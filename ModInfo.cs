using CsvModsExporter;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace CsvModsExporter
{
    public class ModCollection
    {
        public int ModListVersion { get; set; }
        public IEnumerable<ModInfo> ModList { get; set; }
    }

    public class ModInfo
    {
        [ModInfoConf("Mod ID")]
        public string Modid { get; set; }
        [ModInfoConf(IsChecked = true)]
        public string Name { get; set; }
        [ModInfoConf("Mod Version", true)]
        public string Version { get; set; }
        [ModInfoConf("Minecraft Version", true)]
        public string Mcversion { get; set; }
        [ModInfoConf("URL", true)]
        public string Url { get; set; }
        [ModInfoConf(IsChecked = false)]
        public string Credits { get; set; }
        [ModInfoConf("Author List")]
        public List<string> AuthorList { get; set; }
        public string Description { get; set; }
        [ModInfoConf(Name = "LogoFile", IsChecked = false)]
        public string LogoFile { get; set; }
        [ModInfoConf(Name = "Update URL", IsChecked = false)]
        public string UpdateUrl { get; set; }
        [ModInfoConf(IsChecked = false)]
        public string Parent { get; set; }
        [ModInfoConf(Hidden = true)]
        public List<string> Dependencies { get; set; }
        [ModInfoConf(Hidden = true)]
        public List<string> Screenshots { get; set; }
        [ModInfoConf(Name = "Dependencies")]

        public string DependenciesList => Dependencies != null ? string.Join(", ", Dependencies.ToArray()) : string.Empty;
        [ModInfoConf(Name = "Screenshots")]
        public string ScreenshotsList => Screenshots != null ? string.Join(", ", Screenshots.ToArray()) : string.Empty;

        // -----------------

        internal FileInfo JarFile = null;
        private List<string> AvailableFor = new List<string>();

        [ModInfoConf("Available For Versions")]
        public string AvailableForVersions => AvailableFor != null ? string.Join(", ", AvailableFor.ToArray()) : "";

        [ModInfoConf("Actual Size")]
        public string ActualFileSize => JarFile != null ? JarFile.Length+"" : "";

        [ModInfoConf("File Size")]
        public string FileSize => JarFile != null ? BytesToString(JarFile.Length) : "";

        [ModInfoConf("JAR Name")]
        public string ActualFileName => JarFile?.Name ?? "";

        internal void SetJarFile(FileInfo jarFile)
        {
            JarFile = jarFile;
        }

        internal void TryGetAvailableFor()
        {
            try
            {
                if (UpdateUrl?.Length > 0 && UpdateUrl?.EndsWith(".json") == true)
                {
                    using (HttpClient client = new HttpClient())
                    {
                        string url = client.GetStringAsync(UpdateUrl.Replace("github.com", "raw.githubusercontent.com")).Result;
                        UpdateInfo info = JsonConvert.DeserializeObject<UpdateInfo>(url);
                        if (info == null)
                            return;

                        AvailableFor = info.GetPromos().Keys.ToList().Select(s => s.Substring(0, s.IndexOf("-"))).Distinct().ToList();
                        AvailableFor.Sort();
                    }
                }
            }
            catch (Exception)
            {
                return;
            }
        }

        static string BytesToString(long byteCount)
        {
            string[] suffix = { "B", "kB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
            if (byteCount == 0)
                return "0 " + suffix[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return $"{(Math.Sign(byteCount) * num).ToString()} {suffix[place]}";
        }
    }
}
