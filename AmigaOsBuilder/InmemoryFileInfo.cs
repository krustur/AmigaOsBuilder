using System;

namespace AmigaOsBuilder
{
    internal class InmemoryFileInfo : IFileInfo
    {
        private readonly string _content;

        public InmemoryFileInfo(string content)
        {
            _content = content;
        }

        public bool Exists { get; set; }

        public DateTime LastWriteTime { get; set; }

        public long Length { get; set; }

        public IStream OpenRead()
        {
            return new InmemorySystemStream(_content);
        }
    }
}