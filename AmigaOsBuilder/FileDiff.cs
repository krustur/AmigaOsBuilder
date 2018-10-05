namespace AmigaOsBuilder
{
    internal enum FileDiff
    {
        Unknown = 0,
        Equal,
        DiffTargetMissing,
        DiffSourceMissing,
        DiffTargetNewer,
        DiffSourceNewer,
        DiffContent,
    }
}