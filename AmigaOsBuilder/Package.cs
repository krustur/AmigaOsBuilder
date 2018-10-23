namespace AmigaOsBuilder
{
    public class Package
    {
        public bool Include { get; set; }
        public string Path { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string Source { get; set; }

        public override string ToString()
        {
            return $"{Path}";
        }
    }
}