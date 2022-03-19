using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CsvModsExporter
{
    public partial class frmErrors : Form
    {
        public frmErrors(Dictionary<string, Exception> errors)
        {
            InitializeComponent();

            List<ListViewItem> items = new List<ListViewItem>();
            foreach (var kvp in errors)
            {
                ListViewItem item = new ListViewItem(kvp.Key);
                item.SubItems.Add(kvp.Value.Message);
                items.Add(item);
            }

            listView1.Items.AddRange(items.OrderBy(i => i.SubItems[0].Text).ToArray());
            listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
        }

        private void frmErrors_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.None;
            string stringRepresentation = "";

            if (listView1.SelectedItems.Count == 0)
                return;

            for (int i = 0; i < listView1.SelectedItems.Count; i++)
            {
                stringRepresentation += listView1.SelectedItems[i].SubItems[0].Text
                    + Environment.NewLine
                    + "\t" + listView1.SelectedItems[i].SubItems[1].Text;

                if (i < (listView1.SelectedItems.Count - 1))
                    stringRepresentation += "\n\n";
            }

            Clipboard.SetText(stringRepresentation);
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            button3.Enabled = true;
        }
    }
}
