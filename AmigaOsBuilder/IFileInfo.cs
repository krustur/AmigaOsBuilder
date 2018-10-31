using System;

namespace AmigaOsBuilder
{
    public interface IFileInfo
    {
        bool Exists { get; }
        DateTime LastWriteTime { get; }
        long Length { get; }
        Attributes Attributes { get; }

        IStream OpenRead();
    }
}