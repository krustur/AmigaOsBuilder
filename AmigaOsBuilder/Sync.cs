namespace AmigaOsBuilder
{
    public class Sync
    {
        public Sync()
        {
            
        }
        public SyncType SyncType { get; set; }
        public FileType FileType { get; set; }
        public string PackageContentBasePath { get; set; }
        public string SourcePath { get; set; }
        public string TargetPath { get; set; }

        public override string ToString()
        {
            switch (SyncType)
            {
                case SyncType.SourceToTarget:
                {
                    return $@"{SyncType} ({FileType}): {SourcePath} => {TargetPath}";
                }
                case SyncType.TargetToSource:
                {
                    return $@"{SyncType} ({FileType}): {SourcePath} <= {TargetPath}";
                }
                case SyncType.DeleteTarget:
                {
                    return $@"{SyncType} ({FileType}): {TargetPath}";
                }
            }
            return $"i am error (unknown SyncType)";
        }
    }
}