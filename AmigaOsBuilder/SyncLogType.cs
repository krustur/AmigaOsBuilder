using System.ComponentModel;

namespace AmigaOsBuilder
{
    internal enum SyncLogType
    {
        Unknown = 0,
        [Description("Copy To Target ..")]
        CopyToTarget,
        [Description("..... from Source")]
        CopyFromSource,
        [Description("Copy to Source ..")]
        CopyToSource,
        [Description("..... from Target")]
        CopyFromTarget,
        [Description("Delete Target ...")]    
        DeleteTarget,
        [Description("Create Target Dir")]
        CreateTargetDirectory,
        [Description("Create Source Dir")]
        CreateSourceDirectory,
        [Description("Delete Target Dir")]
        DeleteTargetDirectory
    }
}