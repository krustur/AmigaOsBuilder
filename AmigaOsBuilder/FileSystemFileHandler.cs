﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Serilog.Core;

namespace AmigaOsBuilder
{
    public class FileSystemFileHandler : IFileHandler
    {
        private readonly Logger _logger;

        public FileSystemFileHandler(Logger logger, string outputBasePath)
        {
            _logger = logger;
            OutputBasePath = outputBasePath;
        }

        public void CreateBasePaths()
        {
            _logger.Information("Creating output alias directories ...");
            Directory.CreateDirectory(OutputBasePath);
            foreach (var alias in AliasService.GetAliases())
            {
                var aliasPath = AliasService.GetAliasPath(alias);
                var outputAliasPath = Path.Combine(OutputBasePath, aliasPath);
                _logger.Information($@"Alias [{alias}] = [{outputAliasPath}]");
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

        public void FileWriteAllText(string path, string content)
        {
            var fullPath = GetFullPath(path);
            EnsurePathExists(fullPath);
            File.WriteAllText(fullPath, content);
        }

        public void FileCopy(IFileHandler sourceFileHandler, string syncSourcePath, string path)
        {
            var fullPath = GetFullPath(path);
            EnsurePathExists(fullPath);
            var bytes = sourceFileHandler.FileReadAllBytes(syncSourcePath);
            fullPath = WinFixPath(fullPath);
            File.WriteAllBytes(fullPath, bytes);
        }

        private string WinFixPath(string path)
        {

            var invalidFileNames = new List<string>
            {
                "CON", "PRN", "AUX", "NUL", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9",
            };
            var fileCompare = Path.GetFileName(path).ToUpperInvariant();
            if (invalidFileNames.Contains(fileCompare))
            {
                var dir = Path.GetDirectoryName(path);
                var file = Path.GetFileName(path);
                path = Path.Combine(dir, $"_amiga_{file}");
            }

            foreach (var invalidFileName in invalidFileNames)
            {
                if (fileCompare.StartsWith(invalidFileName + "."))
                {
                    var dir = Path.GetDirectoryName(path);
                    var file = Path.GetFileName(path);
                    path = Path.Combine(dir, $"_amiga_{file}");
                    break;
                }
            }

            return path;
        }

        private void EnsurePathExists(string fullPath)
        {
            var directoryName = Path.GetDirectoryName(fullPath);
            Directory.CreateDirectory(directoryName);
        }

        public void FileCopyBack(string path, IFileHandler contentFileHandler, string contentPath)
        {
            var fullPath = GetFullPath(path);
            EnsurePathExists(fullPath);
            var bytes = contentFileHandler.FileReadAllBytes(contentPath);
            File.WriteAllBytes(fullPath, bytes);
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
            
            var packageEntryFileInfo = new FileInfo(fullPath);
            var fileType = (packageEntryFileInfo.Attributes & FileAttributes.Directory) == FileAttributes.Directory ? FileType.Directory : FileType.File;

            return fileType;

            //var fileType = Program.GetFileType(SyncType.Unknown, fullPath);
            //return fileType;


        }

        public IFileInfo GetFileInfo(string path)
        {
            var fullPath = GetFullPath(path);
            var fileInfo = new FileInfo(fullPath);
            return new FileSystemFileInfo(fileInfo);
        }

        public IList<string> DirectoryGetDirectories(string path)
        {
            var fullPath = GetFullPath(path);

            var directories = Directory.GetDirectories(fullPath);

            var fixedDirectories = directories
                .Select(GetSubPath)
                .ToList();

            return fixedDirectories;
        }

        public string OutputBasePath { get; }
        public byte[] FileReadAllBytes(string path)
        {
            var fullPath = GetFullPath(path);

            var bytes = File.ReadAllBytes(fullPath);

            return bytes;
        }

        private string GetSubPath(string fullPath)
        {
            var subPath = Program.RemoveRoot(OutputBasePath, fullPath);
            return subPath;
        }

        private string GetFullPath(string path)
        {
            var fullPath = Path.Combine(OutputBasePath, path);
            return fullPath;
        }

        public IList<string> DirectoryGetFiles(string path)
        {
            var fullPath = Path.Combine(OutputBasePath, path);

            var files = Directory.GetFiles(fullPath);

            return files;
        }

        public DateTime GetDate(string path)
        {
            var fullPath = Path.Combine(OutputBasePath, path);

            var fileInfo = new FileInfo(fullPath);

            var dateTime = fileInfo.LastWriteTime;

            return dateTime;
        }

        public void Dispose()
        {
            //_logger?.Dispose();
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

        public long Length => _fileInfo.Length;

        public IStream OpenRead()
        {
            return new FileSystemStream(_fileInfo.OpenRead());
        }
    }
}