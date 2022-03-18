using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsvModsExporter
{
    internal static class Output
    {
        private static void CleanModDetails(ref List<ModInfo> mods)
        {
            foreach(ModInfo m in mods)
            {
                m.Version = m.Version?.StartsWith("$") == true ? m.Version = "" : m.Version;
                m.Mcversion = m.Mcversion?.StartsWith("$") == true ? m.Mcversion = "" : m.Mcversion;
                m.Description = m.Description?.StartsWith("http") == true ? "" : m.Description;
                m.Description = m.Description?.Replace(Environment.NewLine, " ").Replace('\n', ' ');
                m.Url = (m.Url?.StartsWith("http") == true || m.Url?.StartsWith("https") == true) ? m.Url : "";

                while (m.Description?.Contains("  ") == true)
                    m.Description = m.Description.Replace("  ", " ");
            }
        }

        internal static void PackageMods(List<ModInfo> allMods, bool autoOpen)
        {
            throw new NotImplementedException();
        }

        internal static void ExportMods(List<ModInfo> allMods, bool autoOpen, string outputFile)
        {
            //clean up mod versions
            CleanModDetails(ref allMods);

            using (var writer = new StreamWriter(outputFile))
            {
                CsvWriter csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
                csv.Context.RegisterClassMap<ModInfoMap>();
                csv.WriteRecords(allMods);
                csv.Dispose();
            }

            if (autoOpen)
                Process.Start("explorer", outputFile);
        }

        public sealed class ModInfoMap : ClassMap<ModInfo>
        {
            public ModInfoMap()
            {
                //order relevant columns
                Map(m => m.Name).Index(0);
                Map(m => m.Version).Index(1);
                Map(m => m.Mcversion).Index(2);
                Map(m => m.AvailableForVersions).Index(3);
                Map(m => m.Url).Index(4);
                Map(m => m.Description).Index(5);
                Map(m => m.ActualFileName).Index(6);
                Map(m => m.FileSize).Index(7);
                Map(m => m.ActualFileSize).Index(8);

                Map(m => m.FileSize).Name("Size");
                Map(m => m.ActualFileSize).Name("Actual size");
                Map(m => m.ActualFileName).Name("JAR Name");
                Map(m => m.Version).Name("Mod Version");
                Map(m => m.Mcversion).Name("Minecraft Version");
                Map(m => m.AvailableForVersions).Name("Available For Versions");
                Map(m => m.Url).Name("URL");

                //ignore irrelevant columns
                Map(m => m.Modid).Ignore();
                Map(m => m.Parent).Ignore();
                Map(m => m.Screenshots).Ignore();
                Map(m => m.LogoFile).Ignore();
                Map(m => m.Credits).Ignore();
                Map(m => m.Dependencies).Ignore();
                Map(m => m.UpdateUrl).Ignore();
            }
        }
    }
}
