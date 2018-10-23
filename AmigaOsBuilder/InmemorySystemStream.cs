using System.IO;
using System.Text;

namespace AmigaOsBuilder
{
    internal class InmemorySystemStream : IStream
    {
        private string _content;
        private readonly long _contentLength;
        private long _totalCount;
        private MemoryStream _stream;

        public InmemorySystemStream(string content)
        {
            _content = content;
            _contentLength = _content.Length;
            _totalCount = 0;

            byte[] byteArray = Encoding.UTF8.GetBytes(_content);
            _stream = new MemoryStream(byteArray);
        }

        public void Dispose()
        {
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            if (_totalCount > _contentLength)
            {
                return 0;
            }

            var readCount = _stream.Read(buffer, offset, count);
            _totalCount += readCount;
            if (_totalCount > _contentLength)
            {
                return (int)(readCount - (_totalCount - _contentLength));
            }
            return readCount;
        }
    }
}