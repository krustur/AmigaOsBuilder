using System;

namespace AmigaOsBuilder
{
    public interface IStream : IDisposable
    {
        int Read(byte[] buffer, int offset, int count);
    }
}