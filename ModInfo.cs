using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ModsCsvExporter
{
    public class ModCollection
    {
        public int? ModListVersion { get; set; }
        public IEnumerable<ModInfo>? ModList { get; set; }
    }

    public class ModInfo
    {
        public string? Modid { get; set; }
        public string? Name { get; set; }
        public string? Version { get; set; }
        public string? Mcversion { get; set; }
        public string? Url { get; set; }
        public string? Credits { get; set; }
        public List<string>? AuthorList { get; set; }
        public string? Description { get; set; }
        public string? LogoFile { get; set; }
        public string? UpdateUrl { get; set; }
        public string? Parent { get; set; }
        public List<string>? Dependencies { get; set; }
        public List<object>? Screenshots { get; set; }

        internal FileInfo? JarFile = null;

        private List<string> AvailableFor = new List<string>();

        public string AvailableForVersions => AvailableFor != null ? string.Join(", ", AvailableFor.ToArray()) : "";

        public string ActualFileSize => JarFile != null ? JarFile.Length+"" : "";

        public string FileSize => JarFile != null ? BytesToString(JarFile.Length) : "";

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
                    using (HttpClient client = new())
                    {
                        string url = client.GetStringAsync(UpdateUrl.Replace("github.com", "raw.githubusercontent.com")).Result;
                        UpdateInfo? info = JsonConvert.DeserializeObject<UpdateInfo>(url);
                        if (info == null)
                            return;

                        AvailableFor = info.GetPromos().Keys.ToList().Select(s => s.Substring(0, s.IndexOf("-"))).Distinct().ToList();
                        AvailableFor.Sort();
                    }
                }
            }
            catch (Exception ex)
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
