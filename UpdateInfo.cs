namespace ModsCsvExporter
{
    internal class UpdateInfo
    {
        public string? Homepage { get; set; }
        public Dictionary<string, string>? Promos { get; set; }

        internal Dictionary<string, string> GetPromos()
        {
            return Promos ?? new Dictionary<string, string>();
        }
    }
}