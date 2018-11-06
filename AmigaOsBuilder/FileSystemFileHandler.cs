using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Serilog.Core;

namespace AmigaOsBuilder
{
    public class FileSystemFileHandler : IFileHandler
    {
        private readonly Logger _logger;
        private readonly WinPathFixer _winPathFixer;
        public string OutputBasePath { get; }

        public FileSystemFileHandler(Logger logger, string outputBasePath)
        {
            _logger = logger;
            _winPathFixer = new WinPathFixer();
            OutputBasePath = outputBasePath;
        }

        public void CreateBasePaths(AliasService aliasService)
        {
            _logger.Information("Creating output alias directories ...");
            Directory.CreateDirectory(OutputBasePath);
            foreach (var alias in aliasService.GetAliases())
            {
                var aliasPath = aliasService.GetAliasPath(alias);
                var outputAliasPath = Path.Combine(OutputBasePath, aliasPath);
                _logger.Information("Alias [{Alias}] = [{OutputAliasPath}]", alias, outputAliasPath);
                Directory.CreateDirectory(outputAliasPath);
            }

            _logger.Information("Create output alias directories done!");
        }

        public bool FileExists(string path)
        {
            var fullPath = GetFullPath(path);
            return File.Exists(fullPath);
        }

        public string FileReadAllText(string path)
        {
            var fullPath = GetFullPath(path);
            return File.ReadAllText(fullPath);
        }

        //public void FileWriteAllText(string path, string content)
        //{
        //    var fullPath = GetFullPath(path);
        //    EnsurePathExists(fullPath);
        //    File.WriteAllText(fullPath, content);
        //    //TODO: Write attrib/date
        //}

        public void FileCopy(IFileHandler sourceFileHandler, string syncSourcePath, string path)
        {
            var fullPath = GetFullPath(path);
            EnsurePathExists(fullPath);
            var bytes = sourceFileHandler.FileReadAllBytes(syncSourcePath);
            //fullPath = _winPathFixer.FixPath(fullPath);
            File.WriteAllBytes(fullPath, bytes);
            //TODO: Write attrib/date
        }

        public void FileCopyBack(string path, IFileHandler contentFileHandler, string contentPath)
        {
            var fullPath = GetFullPath(path);
            EnsurePathExists(fullPath);
            var bytes = contentFileHandler.FileReadAllBytes(contentPath);
            //var getDate = contentFileHandler.GetDate(contentPath);
            File.WriteAllBytes(fullPath, bytes);
            //TODO: Write attrib/date
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
            //TODO: Write attrib/date
        }

        public void DirectoryDelete(string path, bool recursive)
        {
            var fullPath = GetFullPath(path);
            Directory.Delete(fullPath, recursive);
        }       

        public FileType GetFileType(string path)
        {
            var fullPath = GetFullPath(path);
            
            var packageEntryFileInfo = new FileInfo(fullPath);
            var fileType = (packageEntryFileInfo.Attributes & FileAttributes.Directory) == FileAttributes.Directory ? FileType.Directory : FileType.File;

            return fileType;
        }

        public IFileInfo GetFileInfo(string path)
        {
            var fullPath = GetFullPath(path);
            var fileInfo = new FileInfo(fullPath);
            return new FileSystemFileInfo(fileInfo);
        }

       

        public byte[] FileReadAllBytes(string path)
        {
            var fullPath = GetFullPath(path);

            var bytes = File.ReadAllBytes(fullPath);

            return bytes;
        }

        public (DateTime DateTime, Attributes Attributes) GetDate(string path)
        {
            var fullPath = GetFullPath(path);

            var fileInfo = new FileInfo(fullPath);

            var dateTime = fileInfo.LastWriteTime;

            return (dateTime, new Attributes(0x00));
            //return (dateTime, new Attributes(fileInfo.Attributes));
        }

        public IList<string> DirectoryGetFileSystemEntriesRecursive(string path)
        {
            var fullPath = GetFullPath(path);

            var entries = Directory.GetFileSystemEntries(fullPath, "*", SearchOption.AllDirectories);

            //TODO: Defix
            var fixedEntries = entries
                .Select(GetSubPath)
                .ToList();

            return fixedEntries;
        }

        //public IList<string> DirectoryGetDirectories(string path)
        //{
        //    var fullPath = GetFullPath(path);

        //    var directories = Directory.GetDirectories(fullPath);

        //    //TODO: Defix
        //    var fixedDirectories = directories
        //        .Select(GetSubPath)
        //        .ToList();

        //    return fixedDirectories;
        //}

        //public IList<string> DirectoryGetFiles(string path)
        //{
        //    var fullPath = GetFullPath(path);

        //    //TODO: Defix
        //    var files = Directory.GetFiles(fullPath);

        //    return files;
        //}

        public void Dispose()
        {
            //_logger?.Dispose();
        }


        private void EnsurePathExists(string fullPath)
        {
            var directoryName = Path.GetDirectoryName(fullPath);
            Directory.CreateDirectory(directoryName);
        }

        private string GetSubPath(string fullPath)
        {
            //TODO: Defix?
            var subPath = _winPathFixer.DefixPath( Program.RemoveRoot(OutputBasePath, fullPath));
            return subPath;
        }

        private string GetFullPath(string path)
        {
            var fullPath = Path.Combine(OutputBasePath, path);
            fullPath = _winPathFixer.FixPath(fullPath);
            return fullPath;
        }
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
        public DateTime LastWriteTime
        {
            get
            {
                var lastWriteTime = _fileInfo.LastWriteTime;
                return lastWriteTime;
            }
        }
        public Attributes Attributes
        {
            get
            {
                return new Attributes(0x00);
                //return new Attributes(_fileInfo.Attributes);
            }
        }

        public long Length => _fileInfo.Length;

        public IStream OpenRead()
        {
            return new FileSystemStream(_fileInfo.OpenRead());
        }   
    }
}