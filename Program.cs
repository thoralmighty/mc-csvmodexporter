// See https://aka.ms/new-console-template for more information

using ModsCsvExporter;
using Newtonsoft.Json;
using System.IO.Compression;

IEnumerable<ModInfo>? GetMetadata(string jarfile)
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

Console.WriteLine("Reading mods...");

List<ModInfo> allMods = new List<ModInfo>();
List<string[]> table = new List<string[]>();

string[] fileList = FindModJars();

/// <summary>
/// Returns a collection of full paths to *.jar files.
/// </summary>
string[] FindModJars()
{
    DirectoryInfo thisDirectory = new DirectoryInfo(".");
    DirectoryInfo? modsDirectory = thisDirectory.GetDirectories().FirstOrDefault(d => d.Name == "mods");

    if (modsDirectory != null)
    {
        return Directory.GetFiles(modsDirectory.FullName, "*.jar");
    }
    else
    {
        return Directory.GetFiles(".", "*.jar");
    }
}

foreach (var file in fileList)
{
    FileInfo jarFile = new FileInfo(file);
    IEnumerable<ModInfo>? modsInFile = GetMetadata(file);

    if (modsInFile == null) continue;

    foreach(ModInfo mod in modsInFile)
    {
        string name = (mod?.Name?.Length > 0 ? mod.Name : $"({jarFile.Name})");

        table.Add(new string[] { name, mod?.Version ?? "" });

        mod?.SetJarFile(jarFile);
        mod?.TryGetAvailableFor();

        if (mod != null) allMods.Add(mod);
    }
}

allMods = allMods.OrderBy(x => x.Name).ToList();

int columnWidth = table.Max(p => p.Max(i => i.Length)) + 5;

foreach (var row in table)
{
    string thisRow = "";
    foreach (var cell in row)
    {
        thisRow += cell.Substring(0, Math.Min(cell.Length, columnWidth));

        int difference = (columnWidth - cell.Length);

        if (difference <= 0)
        {
            thisRow = thisRow.Substring(0, thisRow.Length - 3) + "... ";
        }

        if (difference > 0)
        {
            thisRow += new string(' ', difference + 1);
        }
    }
    Console.WriteLine(thisRow);
}

Console.WriteLine();
Console.WriteLine(allMods.Count + " mods found!");

Output.ExportMods(allMods, Environment.GetCommandLineArgs().Contains("/open"));

//Console.WriteLine("Do you wish to [E]xport a manifest, [P]ackage, [B]oth or [C]ancel?: ");

//Operations? operation = null;
//while (operation == null)
//{
//    var key = Console.ReadKey();

//    if (key.Key == ConsoleKey.E)
//        operation = Operations.Export;
//    else if (key.Key == ConsoleKey.P)
//        operation = Operations.Package;
//    else if (key.Key == ConsoleKey.B)
//        operation = Operations.Both;
//    else if (key.Key == ConsoleKey.C)
//        Environment.Exit(0);
//}

//if (operation == Operations.Export || operation == Operations.Both)
//{
//    Output.ExportMods(allMods);
//}

//if (operation == Operations.Package || operation == Operations.Both)
//{
//    Output.PackageMods(allMods);
//}

enum Operations
{
    Export,
    Package,
    Both
}