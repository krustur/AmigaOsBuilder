﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Serilog.Core;
// ReSharper disable InconsistentNaming

namespace AmigaOsBuilder
{
    public class LhaOutputHandler : IOutputHandler, IDisposable
    {
        private const int INITIAL_HEADER_BUFFER_LENGTH = 32;

        private readonly Logger _logger;
        private readonly string _outputBasePath;
        private List<LhaContent> _content;
        private FileStream _fileStream;
        //private long _fileStreamCurrentPos;
        private Crc16 _crc16Calcer;
        private byte[] _headerBuffer = new byte[INITIAL_HEADER_BUFFER_LENGTH];
        private int _headerBufferLength = INITIAL_HEADER_BUFFER_LENGTH;

        public LhaOutputHandler(Logger logger, string outputBasePath)
        {
            _logger = logger;
            _outputBasePath = outputBasePath;
            _crc16Calcer = new Crc16();

            if (File.Exists(outputBasePath))
            {
                _fileStream = new FileStream(outputBasePath, FileMode.Open, FileAccess.ReadWrite);
                _content = ReadContent(_fileStream);
                if (_fileStream.Length > 0)
                {
                    _fileStream.Seek(-1, SeekOrigin.End);
                }
            }
            else
            {
                _fileStream = new FileStream(outputBasePath, FileMode.Create, FileAccess.Write);
                _content = new List<LhaContent>();

            }
        }

        private List<LhaContent> ReadContent(FileStream fileStream)
        {
            var content = new List<LhaContent>();
            while (fileStream.Position < fileStream.Length)
            {
                var readBytes = fileStream.Read(_headerBuffer, 0, 22);
                if (readBytes == 0 || (readBytes > 1 && readBytes < 22))
                {
                    _logger.Error("Abnormal end of archive file!");
                    break;
                }

                var headerLength = _headerBuffer[0];// = (byte)(headerLength - 2);
                if (headerLength == 0)
                {
                    // End of archive
                    break;
                }

                // crc will be written later
                var headerCrc = _headerBuffer[1];
                var length000000FF = _headerBuffer[7]; // little endian
                var length0000FF00 = _headerBuffer[8];
                var length00FF0000 = _headerBuffer[9];
                var lengthFF000000 = _headerBuffer[10];
                var dateTime000000FF = _headerBuffer[15];
                var dateTime0000FF00 = _headerBuffer[16];
                var dateTime00FF0000 = _headerBuffer[17];
                var dateTimeFF000000 = _headerBuffer[18];
                var attribute = _headerBuffer[19];
                //var levelIdentifier = _headerBuffer[20];
                var pathLength = _headerBuffer[21];


                int contentLength = length000000FF;
                contentLength |= length0000FF00 << 8;
                contentLength |= length00FF0000 << 16;
                contentLength |= lengthFF000000 << 24;

                uint dateTimeInt = dateTime000000FF;
                dateTimeInt |= (uint)(dateTime0000FF00 << 8);
                dateTimeInt |= (uint)(dateTime00FF0000 << 16);
                dateTimeInt |= (uint)(dateTimeFF000000 << 24);
                var dateTime = IntToDateTime(dateTimeInt);

                EnsureHeaderBufferLength(headerLength + 2);

                if (headerLength < 22 + pathLength + 2)
                {
                    if (_headerBufferLength < 22 + pathLength + 2)
                    {
                    }
                }

                var streamHeaderPosition = fileStream.Position;
                fileStream.Read(_headerBuffer, 22, pathLength + 2);
                var path = System.Text.Encoding.UTF8.GetString(_headerBuffer, 22, pathLength);

                var calcHeaderCrc = CalcHeaderCrc(2, 2 + headerLength);
                if (calcHeaderCrc != headerCrc)
                {
                    throw new Exception($"Header CRC mismatch: headerCrc={headerCrc} calcHeaderCrc={calcHeaderCrc}");
                }

                var streamContentPosition = fileStream.Position;
                fileStream.Seek(contentLength, SeekOrigin.Current);

                content.Add(new LhaContent(path, dateTime, contentLength, streamHeaderPosition, streamContentPosition));
            }

            return content;
        }

        public void CreateBasePaths()
        {            
        }

        public bool FileExists(string path)
        {
            path = ToAmigaPath(path)
                .ToLowerInvariant();

            var fileExists = _content
                .Any(x => x.Path.ToLowerInvariant() == path);

            return fileExists;
        }

        public string FileReadAllText(string path)
        {
            throw new NotImplementedException();
        }

        public void FileWriteAllText(string path, string content)
        {
            throw new NotImplementedException();
        }

        public void FileCopy(string syncSourcePath, string path, bool overwrite)
        {
            path = ToAmigaPath(path);
            var sourceContent = File.ReadAllBytes(syncSourcePath);
            var sourceFileInfo = new FileInfo(syncSourcePath);
            var dateTime = sourceFileInfo.LastWriteTime;
            var dateTimeInt = DateTimeToInt(dateTime);

            var fileContentCrc = _crc16Calcer.ComputeChecksum(sourceContent);
            var fileContentCrc00FF = (byte)(fileContentCrc & 0x00FF);
            var fileContentCrcFF00 = (byte)((fileContentCrc & 0xFF00) >> 8);
            var headerLength = (byte)(24 + path.Length);
            byte headerCrc = 0x00;
            var sourceContentLength = sourceContent.Length;
            var length000000FF = (byte)(sourceContentLength & 0x000000FF);
            var length0000FF00 = (byte)((sourceContentLength & 0x0000FF00) >> 8);
            var length00FF0000 = (byte)((sourceContentLength & 0x00FF0000) >> 16);
            var lengthFF000000 = (byte)((sourceContentLength & 0xFF000000) >> 24);
            //var dateTime = 0x00000000;
            var dateTime000000FF = (byte)(dateTimeInt & 0x000000FF);
            var dateTime0000FF00 = (byte)((dateTimeInt & 0x0000FF00) >> 8);
            var dateTime00FF0000 = (byte)((dateTimeInt & 0x00FF0000) >> 16);
            var dateTimeFF000000 = (byte)((dateTimeInt & 0xFF000000) >> 24);
            byte attribute = 0x00;
            
            EnsureHeaderBufferLength(headerLength);
            _headerBuffer[0] = (byte)(headerLength - 2);
            // crc will be written later
            //headerBytes[1] = headerCrc;
            _headerBuffer[2] = 0x2D; // -lh0-
            _headerBuffer[3] = 0x6C;
            _headerBuffer[4] = 0x68;
            _headerBuffer[5] = 0x30;
            _headerBuffer[6] = 0x2D;
            _headerBuffer[7] = length000000FF; // little endian
            _headerBuffer[8] = length0000FF00;
            _headerBuffer[9] = length00FF0000;
            _headerBuffer[10] = lengthFF000000;
            _headerBuffer[11] = length000000FF;
            _headerBuffer[12] = length0000FF00;
            _headerBuffer[13] = length00FF0000;
            _headerBuffer[14] = lengthFF000000;
            _headerBuffer[15] = dateTime000000FF;
            _headerBuffer[16] = dateTime0000FF00;
            _headerBuffer[17] = dateTime00FF0000;
            _headerBuffer[18] = dateTimeFF000000;
            _headerBuffer[19] = attribute;
            _headerBuffer[20] = 0x00; // Level identifier
            _headerBuffer[21] = (byte)path.Length;
            for (var i = 0; i < path.Length; i++)
            {
                _headerBuffer[22 + i] = (byte)path[i];
            }
            _headerBuffer[22 + path.Length] = fileContentCrc00FF;
            _headerBuffer[23 + path.Length] = fileContentCrcFF00;

            headerCrc = CalcHeaderCrc(2, headerLength);
            
            _headerBuffer[1] = headerCrc;

            var sourceHeaderPosition = _fileStream.Position;
            _fileStream.Write(_headerBuffer, 0, headerLength);
            var sourceContentPosition = _fileStream.Position;
            _fileStream.Write(sourceContent, 0, sourceContentLength);
            _fileStream.WriteByte(0);
            _fileStream.Seek(-1, SeekOrigin.Current);

            _content.Add(new LhaContent(path, dateTime, sourceContentLength, sourceHeaderPosition, sourceContentPosition));
        }

        private void EnsureHeaderBufferLength(int headerLength)
        {
            if (headerLength > _headerBufferLength)
            {
                _headerBufferLength = headerLength * 2;
                Array.Resize(ref _headerBuffer, _headerBufferLength);
            }
        }

        private byte CalcHeaderCrc(int start, int end)
        {
            byte crc = 0;
            unchecked
            {
                for (var i = start; i < end; i++)
                {

                    crc += _headerBuffer[i];
                }
            }

            return crc;
        }

        private uint DateTimeToInt(DateTime dateTime)
        {
            uint dateTimeInt = 0;

            dateTimeInt |= (uint)((dateTime.Year - 1980) << 25);
            dateTimeInt |= (uint)(dateTime.Month << 21);
            dateTimeInt |= (uint)(dateTime.Day << 16);
            dateTimeInt |= (uint)(dateTime.Hour << 11);
            dateTimeInt |= (uint)(dateTime.Minute << 5);
            dateTimeInt |= (uint)(dateTime.Second / 2);

            return dateTimeInt;
        }

        private DateTime IntToDateTime(uint dateTimeInt)
        {
            var ye = ((dateTimeInt & 0xFE000000) >> 25) + 1980;
            var mo = (dateTimeInt & 0x01E00000) >> 21;
            var da = (dateTimeInt & 0x001F0000) >> 16;
            var ho = (dateTimeInt & 0x0000F800) >> 11;
            var mi = (dateTimeInt & 0x000007E0) >> 5;
            var se = (dateTimeInt & 0x0000001F) << 1;

            var dateTime = new DateTime((int) ye, (int) mo, (int) da, (int) ho, (int) mi, (int) se);

            return dateTime;
        }

        public void FileCopyBack(string path, string syncSourcePath, bool overwrite)
        {
            throw new NotImplementedException();
        }

        public void FileDelete(string path)
        {
            throw new NotImplementedException();
        }

        public bool DirectoryExists(string path)
        {
            path = ToAmigaPath(path)
                       .ToLowerInvariant()
                   + '/';
            if (_content.Any(x => x.Path.ToLowerInvariant().StartsWith(path)))
            {
                return true;
            }
            return false;
        }

        public void DirectoryCreateDirectory(string path)
        {
        }

        public void DirectoryDelete(string path, bool recursive)
        {
            if (recursive == false)
            {
                throw new NotImplementedException("Non-recursive deletes is not implemented!");
            }
            throw new NotImplementedException();
        }

        public IList<string> DirectoryGetFileSystemEntriesRecursive(string path)
        {
            path = ToAmigaPath(path)
                       .ToLowerInvariant()
                   //+ '/'
                ;

            var fileSystemEntries = _content
                .Where(x => x.Path.ToLowerInvariant().StartsWith(path.ToLowerInvariant()))
                .Select(x => ToWindowsPath(x.Path))
                .ToList();

            return fileSystemEntries;
        }

        public FileType GetFileType(string path)
        {
            path = ToAmigaPath(path)
                .ToLowerInvariant();

            var firstFile = _content
                .FirstOrDefault(x => x.Path.ToLowerInvariant() == path);

            if (firstFile != null)
            {
                return FileType.File;
            }

            var contents = _content
                .Where(x => x.Path.ToLowerInvariant().StartsWith(path));

            if (contents.Any())
            {
                return FileType.Directory;
            }

            return FileType.Unknown;
        }

        public IFileInfo GetFileInfo(string path)
        {

            path = ToAmigaPath(path)
                .ToLowerInvariant();
            var content = _content.SingleOrDefault(x => x.Path.ToLowerInvariant() == path);
            //var arne = new FileInfo("c:\\knutsomintefinns.töst");
            var fileInfo = new LhaContentFileInfo(content, _fileStream);
            return fileInfo;
        }

        public void Dispose()
        {
            _fileStream?.Dispose();
        }

        private string ToAmigaPath(string path)
        {
            var amigaPath = path.Replace('\\', '/');
            return amigaPath;
        }

        private string ToWindowsPath(string path)
        {
            var amigaPath = path.Replace('/', '\\');
            return amigaPath;
        }
    }

    public class LhaContentFileInfo : IFileInfo
    {
        private readonly LhaContent _content;
        private readonly Stream _stream;

        public LhaContentFileInfo(LhaContent content, Stream stream)
        {
            _content = content;
            _stream = stream;
        }

        public bool Exists => _content != null;

        public DateTime LastWriteTimeUtc => _content?.Date ?? DateTime.MinValue;

        public long Length => _content?.Length ?? 0;

        public IStream OpenRead()
        {
            _stream.Seek(_content.StreamContentPosition, SeekOrigin.Begin);
            return new LhaContentStream(_stream, _content.Length);
        }
    }

    public class LhaContent
    {
        public LhaContent(string path, DateTime date, long length, long streamHeaderPosition, long streamContentPosition)
        {
            Path = path;
            Date = date;
            Length = length;
            StreamHeaderPosition = streamHeaderPosition;
            StreamContentPosition = streamContentPosition;
        }
        public string Path { get; set; }
        public DateTime Date { get; set; }
        public long Length { get; set; }
        public long StreamHeaderPosition { get; set; }
        public long StreamContentPosition { get; set; }
    }

    public class LhaContentStream : IStream
    {
        private readonly Stream _stream;
        private readonly long _contentLength;
        private long _totalCount;

        public LhaContentStream(Stream stream, long contentLength)
        {
            _stream = stream;
            _contentLength = contentLength;
            _totalCount = 0;
        }

        public void Dispose()
        {
            // do nothing!
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
                return (int) (readCount - (_totalCount - _contentLength));
            }
            return readCount;
        }
    }
}