// See https://aka.ms/new-console-template for more information

using ModsCsvExporter;
using Newtonsoft.Json;
using System.IO.Compression;


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