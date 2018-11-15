namespace AmigaOsBuilder
{
    public class Package
    {
        public Package() { }

        public Package(bool include, string path)
        {
            Include = include;
            Path = path;
        }

        public bool Include { get;  }
        public string Path { get; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string Source { get; set; }

        public override string ToString()
        {
            return $"{Path}";
        }
    }
}