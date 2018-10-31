using System;

namespace AmigaOsBuilder
{
    internal class InmemoryFileInfo : IFileInfo
    {
        private readonly string _content;
        private readonly bool _exists;
        private readonly DateTime _lastWriteTime;
        private readonly Attributes _attributes;
        private readonly long _length;

        public InmemoryFileInfo(string content, bool exists, DateTime lastWriteTime, Attributes attributes, long length)
        {
            _content = content;
            _exists = exists;
            _lastWriteTime = lastWriteTime;
            _attributes = attributes;
            _length = length;
        }

        public bool Exists
        {
            get
            {
                return _exists;
            }
        }

        public DateTime LastWriteTime
        {
            get
            {
                return _lastWriteTime;
            }
        }
        public Attributes Attributes
        {
            get
            {
                return _attributes;
            }
        }
        public long Length
        {
            get
            {
                return _length;
            }
        }

        public IStream OpenRead()
        {
            return new InmemorySystemStream(_content);
        }
    }
}