using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CsvModsExporter
{
    public partial class Form1 : Form
    {
        //loaded settings
        SettingsObject lastSettings;
        bool anythingChanged = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            AutoSizeColumn();
            LoadSettings();
            ReloadPropertyList(true);
            anythingChanged = false;
        }

        private void AutoSizeColumn()
        {
            listView1.Columns[0].Width = (int)(listView1.Width * 0.8);
        }

        private void ReloadPropertyList(bool useSaved)
        {
            if (lastSettings == null)
                useSaved = false;

            listView1.Items.Clear();
            foreach (var prop in typeof(ModInfo).GetProperties())
            {
                object[] attrs = prop.GetCustomAttributes(true);
                string readablePropName = prop.Name;
                bool shouldCheck = false;
                bool shouldHide = false;

                foreach (object attr in attrs)
                {
                    ModInfoConfAttribute confAttr = attr as ModInfoConfAttribute;
                    if (confAttr != null)
                    {
                        if (confAttr.Name?.Length > 0) readablePropName = confAttr.Name;
                        shouldCheck = confAttr.IsChecked;
                        shouldHide = confAttr.Hidden;
                    }
                }

                if (shouldHide) continue;

                if (useSaved == true && lastSettings != null)
                {
                    shouldCheck = lastSettings.List.FirstOrDefault(c => c.PropertyName == prop.Name)?.Checked == true;
                }

                ListViewItem item = listView1.Items.Add(readablePropName);
                item.Tag = prop;
                listView1.Items[listView1.Items.Count - 1].Checked = shouldCheck;

                listView1.DeselectAll();
            }

            //reorder
            //for (int i = listView1.Items.Count - 1; i >= 0; i--)
            //{
            //    var item = listView1.Items[i];
            //    if (useSaved && lastSettings != null)
            //    {
            //        int targetIndex = lastSettings.List.IndexOf(c => c.PropertyName == ((PropertyInfo)item.Tag).Name);
            //        listView1.Items.Remove(item);
            //        listView1.Items.Insert(targetIndex, item);
            //    }
            //}
        }

        IEnumerable<ModInfo> GetMetadata(string jarfile)
        {
            using (ZipArchive zip = ZipFile.Open(jarfile, ZipArchiveMode.Read))
            {
                foreach (ZipArchiveEntry entry in zip.Entries)
                {
                    if (entry.Name == "mcmod.info")
                    {
                        using (StreamReader reader = new StreamReader(entry.Open()))
                        {
                            string json = reader.ReadToEnd();

                            if (json.Contains("modList"))
                            {
                                return JsonConvert.DeserializeObject<ModCollection>(json)?.ModList;
                            }
                            else
                            {
                                return JsonConvert.DeserializeObject<IEnumerable<ModInfo>>(json);
                            }

                        }
                    }
                }
            }

            //no info found in jarfile
            return new List<ModInfo>();
        }

        /// <summary>
        /// Returns a collection of full paths to *.jar files.
        /// </summary>
        string[] FindModJars(string path)
        {
            DirectoryInfo thisDirectory = new DirectoryInfo(path);
            DirectoryInfo modsDirectory = thisDirectory.GetDirectories().FirstOrDefault(d => d.Name == "mods");

            if (modsDirectory != null)
            {
                return Directory.GetFiles(modsDirectory.FullName, "*.jar");
            }
            else
            {
                return Directory.GetFiles(".", "*.jar");
            }
        }

        internal void Run(string path, string saveAs = null)
        {
            Console.WriteLine("Reading mods...");

            List<ModInfo> allMods = new List<ModInfo>();
            List<string[]> table = new List<string[]>();

            string[] fileList = FindModJars(path);

            foreach (var file in fileList)
            {
                FileInfo jarFile = new FileInfo(file);
                IEnumerable<ModInfo> modsInFile = GetMetadata(file);

                if (modsInFile == null) continue;

                foreach (ModInfo mod in modsInFile)
                {
                    string name = (mod?.Name?.Length > 0 ? mod.Name : $"({jarFile.Name})");

                    table.Add(new string[] { name, mod?.Version ?? "" });

                    mod?.SetJarFile(jarFile);
                    mod?.TryGetAvailableFor();

                    if (mod != null) allMods.Add(mod);
                }
            }

            allMods = allMods.OrderBy(x => x.Name).ToList();

            //int columnWidth = table?.Max(p => p.Max(i => i.Length)) + 5;

            //foreach (var row in table)
            //{
            //    string thisRow = "";
            //    foreach (var cell in row)
            //    {
            //        thisRow += cell.Substring(0, Math.Min(cell.Length, columnWidth));

            //        int difference = (columnWidth - cell.Length);

            //        if (difference <= 0)
            //        {
            //            thisRow = thisRow.Substring(0, thisRow.Length - 3) + "... ";
            //        }

            //        if (difference > 0)
            //        {
            //            thisRow += new string(' ', difference + 1);
            //        }
            //    }
            //    Console.WriteLine(thisRow);
            //}

            Console.WriteLine(allMods.Count + " mods found!");

            Output.ExportMods(allMods, checkBox1.Checked, saveAs); //Environment.GetCommandLineArgs().Contains("/open");

        }

        FolderBrowserDialog dialog;

        private void button1_Click(object sender, EventArgs e)
        {
            if (dialog == null)
            {
                dialog = new FolderBrowserDialog();
                dialog.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);
                dialog.Description = "Select your Minecraft instance, .minecraft or mods folder:";
            }

            if (textBox1.Text.Trim()?.Length > 0 && Directory.Exists(textBox1.Text))
                dialog.SelectedPath = textBox1.Text;

            if (dialog.ShowDialog() == DialogResult.Cancel)
                return;

            if (!IsModsFolder(dialog.SelectedPath))
            {
                if (MessageBox.Show("The selected folder does not appear to be a traditional mods folder or does not yet contain any mods, use it anyway?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                    return;
            }

            textBox1.Text = dialog.SelectedPath;
        }

        private bool IsModsFolder(string selectedPath)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(selectedPath);
            return directoryInfo.Exists && directoryInfo.Name == "mods" && directoryInfo.GetFiles().Any(f => f.Extension == ".jar");
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
        }

        private void button4_Click(object sender, EventArgs e)
        {
            listView1.MoveSelectedItemUp();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            listView1.MoveSelectedItemDown();
        }

        private void listView1_Resize(object sender, EventArgs e)
        {
            AutoSizeColumn();
        }

        SaveFileDialog saveFileDialog1;

        private void button2_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(textBox1.Text))
            {
                MessageBox.Show("Invalid mods folder!", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (checkBox2.Checked == false)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.InitialDirectory = (textBox1.Text);
            }

            string targetFile = textBox1.Text + "\\" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".csv";

            if (checkBox2.Checked == true)
            {
                if (saveFileDialog1 == null)
                {
                    saveFileDialog1 = new SaveFileDialog()
                    {
                        Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                        InitialDirectory = textBox1.Text,
                        Title = "Save as"
                    };
                }
                if (saveFileDialog1.ShowDialog() == DialogResult.Cancel)
                    return;
            }

            Run(textBox1.Text);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            //if (MessageBox.Show("Are you sure?", "", MessageBoxButtons.YesNo) == DialogResult.No)
            //    return;
            ReloadPropertyList(false);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (listView1.CheckedItems.Count == listView1.Items.Count)
            {
                for (int i = 0; i < listView1.Items.Count; i++)
                {
                    listView1.Items[i].Checked = false;
                }
            }
            else
            {
                for (int i = 0; i < listView1.Items.Count; i++)
                {
                    listView1.Items[i].Checked = true;
                }
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            button2.Enabled = textBox1.Text.Length >= 3;
            anythingChanged = true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void textBox1_Leave(object sender, EventArgs e)
        {
        }

        static string settingsFile = new FileInfo(Assembly.GetEntryAssembly().Location).Directory.FullName + "/CsvModsExporter.settings.json";

        private void SaveSettings()
        {
            SettingsObject settings = new SettingsObject(textBox1.Text, checkBox1.Checked, checkBox2.Checked, GetListViewChoices());
            File.WriteAllText(settingsFile, JsonConvert.SerializeObject(settings));
        }

        private void LoadSettings()
        {
            if (!File.Exists(settingsFile))
            {
                button8.Enabled = false;
                return;
            }

            SettingsObject settings = JsonConvert.DeserializeObject<SettingsObject>(File.ReadAllText(settingsFile));
            textBox1.Text = settings.Text;
            checkBox1.Checked = settings.Checked1;
            checkBox2.Checked = settings.Checked2;
            lastSettings = settings;
        }

        List<ListViewChoice> GetListViewChoices()
        {
            return listView1.Items.Cast<ListViewItem>().Select(i => new ListViewChoice(((PropertyInfo)i.Tag).Name, i.Checked)).ToList();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            ReloadPropertyList(true);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (anythingChanged == true)
            {
                switch (MessageBox.Show("Save changes?", "", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
                {
                    case DialogResult.Yes:
                        SaveSettings();
                        break;

                    case DialogResult.Cancel:
                        return;
                }
            }
        }

        private void listView1_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            anythingChanged = true;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            anythingChanged = true;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            anythingChanged = true;
        }
    }

    [Serializable]
    internal class ListViewChoice
    {
        public string PropertyName { get; set; }
        public bool Checked { get; set; }

        public ListViewChoice(string propName, bool @checked)
        {
            this.PropertyName = propName;
            this.Checked = @checked;
        }
    }

    internal class SettingsObject
    {
        public SettingsObject()
        {
        }

        public SettingsObject(string text, bool checked1, bool checked2, List<ListViewChoice> list)
        {
            Text = text;
            Checked1 = checked1;
            Checked2 = checked2;
            List = list;
        }

        public string Text { get; set; }
        public bool Checked1 { get; set; }
        public bool Checked2 { get; set; }
        public List<ListViewChoice> List { get; set; }
    }
}
