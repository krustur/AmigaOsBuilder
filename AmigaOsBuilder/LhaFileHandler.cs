﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Serilog.Core;
// ReSharper disable InconsistentNaming

namespace AmigaOsBuilder
{
    public class LhaFileHandler : IFileHandler, IDisposable
    {
        private const int INITIAL_HEADER_BUFFER_LENGTH = 32;
        private const char FolderSeparator = '\\';

        private readonly Logger _logger;
        public string OutputBasePath { get; }
        public byte[] FileReadAllBytes(string path)
        {
            var content = GetSingleContentByPath(path);
            _fileStreamReadyForAppend = false;
            _fileStream.Seek(content.StreamContentPosition, SeekOrigin.Begin);
            var bytes = new byte[content.Length];
            _fileStream.Read(bytes, 0, content.Length);
            return bytes;
        }

        private readonly List<LhaContent> _content;
        private readonly FileStream _fileStream;
        private bool _fileStreamReadyForAppend;
        private readonly Crc16 _crc16Calcer;
        private readonly EncodingConverter _encodingConverter;
        private byte[] _headerBuffer = new byte[INITIAL_HEADER_BUFFER_LENGTH];
        private int _headerBufferLength = INITIAL_HEADER_BUFFER_LENGTH;

        public LhaFileHandler(Logger logger, string outputBasePath)
        {
            if (outputBasePath.ToLowerInvariant().Contains("output")) { }
            _logger = logger;
            OutputBasePath = outputBasePath;
            _crc16Calcer = new Crc16();
            _encodingConverter = new EncodingConverter();

            if (File.Exists(outputBasePath))
            {
                _fileStream = new FileStream(outputBasePath, FileMode.Open, FileAccess.ReadWrite);
                _content = ReadAllContent(_fileStream);
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

            _fileStreamReadyForAppend = true;
        }

        private List<LhaContent> ReadAllContent(FileStream fileStream)
        {
            var allContent = new List<LhaContent>();
            var keepGoing = true;
            while (keepGoing == true)//fileStream.Position < fileStream.Length)
            {
                var (content, bytes) = ReadContent(fileStream);
                if (content != null)
                {
                    allContent.Add(content);
                }
                else
                {
                    keepGoing = false;
                }
            }

            return allContent;
        }

        private (LhaContent, byte[]) ReadContent(FileStream fileStream, long? position = null)
        {
            if (position.HasValue)
            {
                _fileStreamReadyForAppend = false;
                _fileStream.Seek(position.Value, SeekOrigin.Begin);

            }

            var streamHeaderPosition = fileStream.Position;
            var readBytes = fileStream.Read(_headerBuffer, 0, 22);
            if (readBytes == 0 || (readBytes > 1 && readBytes < 22))
            {
                _logger.Error("Abnormal end of archive file!");
                //break;
                return (null, null);
            }

            var headerLength = _headerBuffer[0];
            if (headerLength == 0)
            {
                // End of archive
                //break;
                return (null, null);
            }

            // crc will be written later
            var headerCrc = _headerBuffer[1];
            //var methodId = LhaEncoding.GetString(_headerBuffer, 2, 5);
            var methodId = _encodingConverter.ConvertIsoBytesToUtf8String(_headerBuffer, 2, 5);
            

            var length000000FF = _headerBuffer[7]; // little endian
            var length0000FF00 = _headerBuffer[8];
            var length00FF0000 = _headerBuffer[9];
            var lengthFF000000 = _headerBuffer[10];
            var dateTime000000FF = _headerBuffer[15];
            var dateTime0000FF00 = _headerBuffer[16];
            var dateTime00FF0000 = _headerBuffer[17];
            var dateTimeFF000000 = _headerBuffer[18];
            var attributes = _headerBuffer[19];
            var levelIdentifier = _headerBuffer[20];
            

            var pathLength = _headerBuffer[21];

            int contentLength = length000000FF;
            contentLength |= length0000FF00 << 8;
            contentLength |= length00FF0000 << 16;
            contentLength |= lengthFF000000 << 24;

            uint dateTimeInt = dateTime000000FF;
            dateTimeInt |= (uint) (dateTime0000FF00 << 8);
            dateTimeInt |= (uint) (dateTime00FF0000 << 16);
            dateTimeInt |= (uint) (dateTimeFF000000 << 24);
            var dateTime = IntToDateTime(dateTimeInt);

            EnsureHeaderBufferLength(headerLength + 2);

            fileStream.Read(_headerBuffer, 22, pathLength + 2);
            var path = _encodingConverter.ConvertIsoBytesToUtf8String(_headerBuffer, 22, pathLength);
            string comment = null;
            for (var i = 0; i < pathLength; i++)
            {
                if (_headerBuffer[22+i] == 0)
                {
                    path = _encodingConverter.ConvertIsoBytesToUtf8String(_headerBuffer, 22, i);
                    comment = _encodingConverter.ConvertIsoBytesToUtf8String(_headerBuffer, 22 + i + 1, pathLength - i - 1);
                }
            }

            var calcHeaderCrc = CalcHeaderCrc(2, headerLength);
            if (methodId != "-lh0-")
            {
                throw new Exception($"Unknown Method ID in [{path}]: {methodId}");
            }
            if (levelIdentifier != 0)
            {
                throw new Exception($"Unknown Level identifier in [{path}]: {levelIdentifier}");
            }
            if (calcHeaderCrc != headerCrc)
            {
                throw new Exception($"Header CRC mismatch in [{path}]: headerCrc={headerCrc} calcHeaderCrc={calcHeaderCrc}");
            }

            var streamContentPosition = fileStream.Position;
            fileStream.Seek(contentLength, SeekOrigin.Current);

            var content = new LhaContent(path, dateTime, attributes, contentLength, streamHeaderPosition, streamContentPosition);

            return (content, _headerBuffer);
        }



        public void CreateBasePaths(AliasService aliasService)
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
            var bytes = FileReadAllBytes(path);
            var text = _encodingConverter.ConvertIsoBytesToUtf8String(bytes);
            return text;
        }

        //public void FileWriteAllText(string path, string content)
        //{
        //    path = ToAmigaPath(path);
        //    var bytes = LhaEncoding.GetBytes(content);
        //    var dateTime = DateTime.Now;
        //    AddLhaContent(path, bytes, dateTime, 0x00);
        //}

        public void FileCopy(IFileHandler sourceFileHandler, string syncSourcePath, string path)
        {
            if (FileExists(path))
            {
                FileDelete(path);
            }
            path = ToAmigaPath(path);
            var sourceBytes = sourceFileHandler.FileReadAllBytes(syncSourcePath);
            //var sourceFileInfo = new FileInfo(syncSourcePath);
            //var dateTime = sourceFileInfo.LastWriteTime;
            var getDate = sourceFileHandler.GetDate(syncSourcePath);
            
            AddLhaContent(path, sourceBytes, getDate.DateTime, getDate.Attributes.GetAmigaAttributes());
        }

        private void AddLhaContent(string path, byte[] bytes, DateTime dateTime, byte attributes)
        {
            var fileContentCrc = _crc16Calcer.ComputeChecksum(bytes);
            var bytesLength = bytes.Length;
   
            var sourceHeaderPosition = WriteLhaHeader(path, dateTime, attributes, fileContentCrc, bytesLength, true);

            var sourceContentPosition = _fileStream.Position;
            _fileStream.Write(bytes, 0, bytesLength);
            _fileStream.WriteByte(0);
            _fileStream.Seek(-1, SeekOrigin.Current);

            _content.Add(new LhaContent(path, dateTime, attributes, bytesLength, sourceHeaderPosition, sourceContentPosition));
        }

        private long WriteLhaHeader(string path, DateTime ?dateTime, byte attributes, ushort? contentCrc, int? bytesLength, bool appendToEnd)
        {
            int headerLength;
            int fullHeaderLength;
            int pathBytesLength;
            if (path != null)
            {
                var pathBytes = _encodingConverter.ConvertUtf8StringToIsoBytes(path);
                pathBytesLength = pathBytes.Length;
                headerLength = (22 + pathBytesLength);
                fullHeaderLength = headerLength + 2;
                EnsureHeaderBufferLength(fullHeaderLength);
                _headerBuffer[21] = (byte)pathBytesLength;
                for (var i = 0; i < pathBytesLength; i++)
                {
                    _headerBuffer[22 + i] = pathBytes[i];
                }
            }
            else
            {
                headerLength = _headerBuffer[0];
                fullHeaderLength = headerLength + 2;
                EnsureHeaderBufferLength(fullHeaderLength);
                pathBytesLength = _headerBuffer[21];
            }

            _headerBuffer[0] = (byte)(headerLength);
            // crc will be written later
            //headerBytes[1] = headerCrc;
            _headerBuffer[2] = 0x2D; // -lh0-
            _headerBuffer[3] = 0x6C;
            _headerBuffer[4] = 0x68;
            _headerBuffer[5] = 0x30;
            _headerBuffer[6] = 0x2D;
            if (bytesLength.HasValue)
            {
                //var bytesLength = bytes.Length;
                var length000000FF = (byte) (bytesLength & 0x000000FF);
                var length0000FF00 = (byte) ((bytesLength & 0x0000FF00) >> 8);
                var length00FF0000 = (byte) ((bytesLength & 0x00FF0000) >> 16);
                var lengthFF000000 = (byte) ((bytesLength & 0xFF000000) >> 24);
                _headerBuffer[7] = length000000FF; // little endian
                _headerBuffer[8] = length0000FF00;
                _headerBuffer[9] = length00FF0000;
                _headerBuffer[10] = lengthFF000000;
                _headerBuffer[11] = length000000FF;
                _headerBuffer[12] = length0000FF00;
                _headerBuffer[13] = length00FF0000;
                _headerBuffer[14] = lengthFF000000;
            }

            if (dateTime.HasValue)
            {
                var dateTimeInt = DateTimeToInt(dateTime.Value);
                var dateTime000000FF = (byte)(dateTimeInt & 0x000000FF);
                var dateTime0000FF00 = (byte)((dateTimeInt & 0x0000FF00) >> 8);
                var dateTime00FF0000 = (byte)((dateTimeInt & 0x00FF0000) >> 16);
                var dateTimeFF000000 = (byte)((dateTimeInt & 0xFF000000) >> 24);
                _headerBuffer[15] = dateTime000000FF;
                _headerBuffer[16] = dateTime0000FF00;
                _headerBuffer[17] = dateTime00FF0000;
                _headerBuffer[18] = dateTimeFF000000;
            }

            
            _headerBuffer[19] = attributes;
            _headerBuffer[20] = 0x00; // Level identifier

            if (contentCrc.HasValue)
            {
                var fileContentCrc00FF = (byte)(contentCrc.Value & 0x00FF);
                var fileContentCrcFF00 = (byte)((contentCrc.Value & 0xFF00) >> 8);
                _headerBuffer[22 + pathBytesLength] = fileContentCrc00FF;
                _headerBuffer[23 + pathBytesLength] = fileContentCrcFF00;
            }

            var headerCrc = CalcHeaderCrc(2, headerLength);

            _headerBuffer[1] = headerCrc;

            if (appendToEnd && _fileStreamReadyForAppend == false)
            {
                _fileStream.Seek(-1, SeekOrigin.End);
                _fileStreamReadyForAppend = true;
            }
            var sourceHeaderPosition = _fileStream.Position;
            _fileStream.Write(_headerBuffer, 0, fullHeaderLength);
            return sourceHeaderPosition;
        }

        private void EnsureHeaderBufferLength(int headerLength)
        {
            if (headerLength > _headerBufferLength)
            {
                _headerBufferLength = headerLength * 2;
                Array.Resize(ref _headerBuffer, _headerBufferLength);
            }
        }

        private byte CalcHeaderCrc(int start, int count)
        {
            byte crc = 0;
            unchecked
            {
                for (var i = start; i < start+count; i++)
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

        public void FileCopyBack(string path, IFileHandler contentFileHandler, string contentPath)
        {
            _logger.Error("LhaFileHandler.FileCopyBack(\"{Path}\"..., \"{ContentPath}\") not implemented", path, contentPath);
        }

        public void FileDelete(string path)
        {
            var content = GetSingleContentByPath(path);
            var (contentRead, bytes) = ReadContent(_fileStream, content.StreamHeaderPosition);
            var pathLength = _headerBuffer[21];
            if (pathLength < 2)
            {
                throw new Exception($"Unable to delete path {path}");
            }

            bytes[22] = (byte) 'd';
            bytes[23] = (byte) FolderSeparator;

            _fileStreamReadyForAppend = false;
            _fileStream.Seek(content.StreamHeaderPosition, SeekOrigin.Begin);
            WriteLhaHeader(path: null, dateTime: null, attributes: 0x00, contentCrc: null, bytesLength: null, appendToEnd: false);
            _content.Remove(content);
        }

        public bool DirectoryExists(string path)
        {
            path = ToAmigaPath(path)
                       .ToLowerInvariant()
                   + FolderSeparator;
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
            //if (recursive == false)
            //{
            //    throw new ArgumentException("Non-recursive deletes is not implemented!");
            //}
            //throw new NotImplementedException();
        }

        public IList<string> DirectoryGetFileSystemEntriesRecursive(string path)
        {
            path = ToAmigaPath(path)
                       .ToLowerInvariant()
                ;

            var startsWidth = $"d{FolderSeparator}";
            var fileSystemEntries = _content
                .Where(x => x.Path.ToLowerInvariant().StartsWith(startsWidth) == false)
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
            var content = GetSingleContentByPath(path);
            var fileInfo = new LhaContentFileInfo(content, GiveAwayStreamAction);
            return fileInfo;
        }

        //public IList<string> DirectoryGetDirectories(string path)
        //{
        //    var amigaPath = ToAmigaPath(path)
        //        .ToLowerInvariant();

        //    var directories = _content
        //        .Where(x => x.Path.ToLowerInvariant().StartsWith(amigaPath))
        //        .Select(x => x.Path.Split(FolderSeparator).First())
        //        .Distinct()
        //        .ToList()
        //        ;

        //    return directories;
        //}

        private LhaContent GetSingleContentByPath(string path)
        {
            path = ToAmigaPath(path)
                .ToLowerInvariant();
            var content = _content.SingleOrDefault(x => x.Path.ToLowerInvariant() == path);
            return content;
        }

        private Stream GiveAwayStreamAction()
        {
            _fileStreamReadyForAppend = false;
            return _fileStream;
        }

        public void Dispose()
        {
            //_logger?.Dispose();
            _fileStream?.Dispose();
        }

        private string ToAmigaPath(string path)
        {
            var amigaPath = path;//.Replace('\\', '/');
            return amigaPath;
        }

        private string ToWindowsPath(string path)
        {
            var amigaPath = path;//.Replace('/', '\\');
            return amigaPath;
        }

        //public IList<string> DirectoryGetFiles(string path)
        //{
        //    var amigaPath = ToAmigaPath(path)
        //        .ToLowerInvariant();

        //    var files = _content
        //        .Where(x => x.Path.ToLowerInvariant().StartsWith(amigaPath))
        //        .Select(x => x.Path.ToLowerInvariant())
        //        .Distinct()
        //        .ToList();

        //    if (amigaPath.Length != 0)
        //    {
        //        files = files
        //            .Select(x => x.Replace(amigaPath, string.Empty))
        //            .Distinct()
        //            .ToList();
        //    }

        //    files = files
        //            .Where(x => x.Contains(FolderSeparator) == false)
        //            .Distinct()
        //            .ToList();

        //    if (files.Count > 0)
        //    {

        //    }
        //    return files;

        //}

        public (DateTime DateTime, Attributes Attributes) GetDate(string path)
        {
            var content = GetSingleContentByPath(path);
            return (content.Date, new Attributes(content.Attributes));
        }
    }

    public class LhaContentFileInfo : IFileInfo
    {
        private readonly LhaContent _content;
        private readonly Func<Stream> _getStreamAction;

        public LhaContentFileInfo(LhaContent content, Func<Stream> getStreamAction)
        {
            _content = content;
            _getStreamAction = getStreamAction;
        }

        public bool Exists => _content != null;

        public DateTime LastWriteTime => _content?.Date ?? DateTime.MinValue;
        public Attributes Attributes
        {
            get
            {
                return new Attributes(_content.Attributes);
            }
        }
        public long Length => _content?.Length ?? 0;

        public IStream OpenRead()
        {
            var stream = _getStreamAction.Invoke();
            stream.Seek(_content.StreamContentPosition, SeekOrigin.Begin);
            return new LhaContentStream(stream, _content.Length);
        }
    }

    public class LhaContent
    {
        public override string ToString()
        {
            return $"{Path}";
        }

        public LhaContent(string path, DateTime date, byte attributes, int length, long streamHeaderPosition, long streamContentPosition)
        {
            Path = path;
            Date = date;
            Attributes = attributes;
            Length = length;
            StreamHeaderPosition = streamHeaderPosition;
            StreamContentPosition = streamContentPosition;
        }
        public string Path { get; set; }
        public DateTime Date { get; set; }
        public byte Attributes { get; set; }
        public int Length { get; set; }
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

    public class EncodingConverter
    {
        public Encoding IsoEncoding = Encoding.GetEncoding("ISO-8859-1");
        public Encoding Utf8Encoding => Encoding.UTF8;

        public string ConvertIsoBytesToUtf8String(byte[] isoBytes)
        {
            var utfBytes = Encoding.Convert(IsoEncoding, Utf8Encoding, isoBytes);
            string str = Utf8Encoding.GetString(utfBytes);
            return str;
            //return ConvertIsoBytesToUtf8String(bytes, 0, bytes.Length);
        }

        public string ConvertIsoBytesToUtf8String(byte[] isoBytes, int offset, int count)
        {
            var isoBytesSub = new byte[count];
            Array.Copy(isoBytes, offset, isoBytesSub, 0, count);
            return ConvertIsoBytesToUtf8String(isoBytesSub);

            //var utfBytes = Encoding.Convert(IsoEncoding, Utf8Encoding, isoBytes);
            //string str = Utf8Encoding.GetString(utfBytes, offset, count);
            //return str;


            //Encoding utf8 = Encoding.UTF8;
            //byte[] utfBytes = utf8.GetBytes(Message);
            //byte[] isoBytes = Encoding.Convert(utf8, iso, utfBytes);


            //Encoding iso = Encoding.GetEncoding("ISO-8859-1");
            //Encoding utf8 = Encoding.UTF8;
            //byte[] utfBytes = utf8.GetBytes(Message);
            //byte[] isoBytes = Encoding.Convert(utf8, iso, utfBytes);
            //string msg = iso.GetString(isoBytes);
        }

        public byte[] ConvertUtf8StringToIsoBytes(string utfString)
        {
            //Encoding iso = Encoding.GetEncoding("ISO-8859-1");
            //Encoding utf8 = Encoding.UTF8;
            byte[] utfBytes = Utf8Encoding.GetBytes(utfString);
            byte[] isoBytes = Encoding.Convert(Utf8Encoding, IsoEncoding, utfBytes);
            return isoBytes;
            //string isoString = IsoEncoding.GetString(isoBytes);
        }
    }
}