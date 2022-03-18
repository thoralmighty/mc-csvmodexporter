using System;

namespace CsvModsExporter
{
    internal class ModInfoConfAttribute : Attribute
    {
        public ModInfoConfAttribute()
        {
        }

        public ModInfoConfAttribute(string name, bool isChecked = false)
        {
            this.Name = name;
            this.IsChecked = isChecked;
        }

        public string Name { get; set; }
        public bool IsChecked { get; set; } = true;
        public bool Hidden { get; set; } = false;
    }
}