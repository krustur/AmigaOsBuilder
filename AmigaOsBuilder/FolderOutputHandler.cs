using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Serilog.Core;

namespace AmigaOsBuilder
{
    public interface IOutputHandler
    {
        void CreateBasePaths();
        bool FileExists(string path);
        string FileReadAllText(string path);
        void FileWriteAllText(string path, string content);
        void FileCopy(string syncSourcePath, string path, bool overwrite);
        void FileCopyBack(string path, string syncSourcePath, bool overwrite);
        void FileDelete(string path);
        bool DirectoryExists(string path);
        void DirectoryCreateDirectory(string path);
        void DirectoryDelete(string path, bool recursive);
        IList<string> DirectoryGetFileSystemEntriesRecursive(string path);
        //string GetSubPath(string fullPath);
        FileType GetFileType(string path);
        IFileInfo GetFileInfo(string path);
    }

    public interface IFileInfo
    {
        bool Exists { get; }
        DateTime LastWriteTimeUtc { get; }
        long Length { get; }

        IStream OpenRead();
    }

    public interface IStream : IDisposable
    {
        int Read(byte[] buffer, int offset, int count);
    }

    public class FileSystemStream : IStream
    {
        private readonly Stream _stream;

        public FileSystemStream(Stream stream)
        {
            _stream = stream;
        }

        public void Dispose()
        {
            _stream?.Dispose();
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            return _stream.Read(buffer, offset, count);
        }
    }

    public class FileSystemFileInfo : IFileInfo
    {
        private readonly FileInfo _fileInfo;

        public FileSystemFileInfo(FileInfo fileInfo)
        {
            _fileInfo = fileInfo;
        }

        public bool Exists => _fileInfo.Exists;
        public DateTime LastWriteTimeUtc => _fileInfo.LastWriteTimeUtc;
        public long Length => _fileInfo.Length;

        public IStream OpenRead()
        {
            return new FileSystemStream(_fileInfo.OpenRead());
        }
    }

    public class FolderOutputHandler : IOutputHandler
    {
        private readonly Logger _logger;
        private readonly string _outputBasePath;

        public FolderOutputHandler(Logger logger, string outputBasePath)
        {
            _logger = logger;
            _outputBasePath = outputBasePath;
        }

        public void CreateBasePaths()
        {
            _logger.Information("Creating output alias directories ...");
            Directory.CreateDirectory(_outputBasePath);
            foreach (var alias in AliasService.GetAliases())
            {
                var aliasPath = AliasService.GetAliasPath(alias);
                var outputAliasPath = Path.Combine(_outputBasePath, aliasPath);
                _logger.Information($@"Alias [{alias}] = [{outputAliasPath}]");
                Directory.CreateDirectory(outputAliasPath);
            }

            _logger.Information("Create output alias directories done!");
        }

        public bool FileExists(string path)
        {
            return File.Exists(GetFullPath(path));
        }

        public string FileReadAllText(string path)
        {
            return File.ReadAllText(GetFullPath(path));
        }

        public void FileWriteAllText(string path, string content)
        {
            var fullPath = GetFullPath(path);
            File.WriteAllText(fullPath, content);
        }

        public void FileCopy(string syncSourcePath, string path, bool overwrite)
        {
            var fullPath = GetFullPath(path);
            File.Copy(syncSourcePath, fullPath, overwrite);
        }

        public void FileCopyBack(string path, string syncSourcePath, bool overwrite)
        {
            var fullPath = GetFullPath(path);
            File.Copy(fullPath, syncSourcePath, overwrite);
        }

        public void FileDelete(string path)
        {
            var fullPath = GetFullPath(path);
            File.Delete(fullPath);
        }

        public bool DirectoryExists(string path)
        {
            var fullPath = GetFullPath(path);
            return Directory.Exists(fullPath);
        }

        public void DirectoryCreateDirectory(string path)
        {
            var fullPath = GetFullPath(path);
            Directory.CreateDirectory(fullPath);
        }

        public void DirectoryDelete(string path, bool recursive)
        {
            var fullPath = GetFullPath(path);
            Directory.Delete(fullPath, recursive);
        }

        public IList<string> DirectoryGetFileSystemEntriesRecursive(string path)
        {
            var fullPath = GetFullPath(path);

            var entries = Directory.GetFileSystemEntries(fullPath, "*", SearchOption.AllDirectories);

            var fixedEntries = entries
                .Select(GetSubPath)
                .ToList();

            return fixedEntries;
        }

        public FileType GetFileType(string path)
        {
            var fullPath = GetFullPath(path);
            var fileType = Program.GetFileType(SyncType.Unknown, fullPath);
            return fileType;
        }

        public IFileInfo GetFileInfo(string path)
        {
            var fullPath = GetFullPath(path);
            var fileInfo = new FileInfo(fullPath);
            return new FileSystemFileInfo(fileInfo);
        }

        private string GetSubPath(string fullPath)
        {
            var subPath = Program.RemoveRoot(_outputBasePath, fullPath);
            return subPath;
        }

        private string GetFullPath(string path)
        {
            var fullPath = Path.Combine(_outputBasePath, path);
            return fullPath;
        }
    }
}