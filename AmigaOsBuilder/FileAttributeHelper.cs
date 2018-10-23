using System;
using System.IO;

namespace AmigaOsBuilder
{
    public class FileAttributeHelper
    {
        public static byte ToAmigaAttributes(FileAttributes winAttributes)
        {
            byte attributes = 0x00;
            if (winAttributes.HasFlag(FileAttributes.Hidden)){ attributes |= 0x80; }
            if (winAttributes.HasFlag(FileAttributes.System)) { attributes |= 0x20; }
            if (!winAttributes.HasFlag(FileAttributes.Archive)) { attributes |= 0x10; }
            if (winAttributes.HasFlag(FileAttributes.ReadOnly)) { attributes |= 0x40; }
            if (winAttributes.HasFlag(FileAttributes.ReadOnly)) { attributes |= 0x10; }

            return attributes;
        }
    }
}