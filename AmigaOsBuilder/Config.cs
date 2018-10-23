using System.Collections.Generic;

namespace AmigaOsBuilder
{
    public class Config
    {
        public string SourceBasePath { get; internal set; }
        public string OutputBasePath { get; internal set; }
        public IDictionary<string, string> Aliases { get; internal set; }
        public IList<Package> Packages { get; internal set; }
        public bool ReverseSync { get; internal set; }
    }
}