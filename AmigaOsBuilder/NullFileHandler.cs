using System;
using System.Collections.Generic;

namespace AmigaOsBuilder
{
    internal class NullFileHandler : IFileHandler
    {
        public NullFileHandler(string outputBasePath)
        {
            OutputBasePath = outputBasePath;
        }

        public string OutputBasePath { get; } 

        public void CreateBasePaths()
        {
            
        }

        public void DirectoryCreateDirectory(string path)
        {
            
        }

        public void DirectoryDelete(string path, bool recursive)
        {
            
        }

        public bool DirectoryExists(string path)
        {
            return true;
        }

        public IList<string> DirectoryGetDirectories(string path)
        {
            return new List<string>();
        }

        public IList<string> DirectoryGetFiles(string path)
        {
            return new List<string>();

        }

        public IList<string> DirectoryGetFileSystemEntriesRecursive(string path)
        {
            return new List<string>();

        }

        public void Dispose()
        {
            
        }

        public void FileCopy(IFileHandler sourceFileHandler, string syncSourcePath, string path)
        {
            
        }

        public void FileCopyBack(string path, string syncSourcePath, bool overwrite)
        {
            
        }

        public void FileDelete(string path)
        {
            
        }

        public bool FileExists(string path)
        {
            return true;            
        }

        public byte[] FileReadAllBytes(string path)
        {
            return null;
        }

        public string FileReadAllText(string path)
        {
            return null;

        }

        public void FileWriteAllText(string path, string content)
        {
            
        }

        public DateTime GetDate(string path)
        {
            return DateTime.MinValue;

        }

        public IFileInfo GetFileInfo(string path)
        {
            return null;

        }

        public FileType GetFileType(string path)
        {
            return FileType.Unknown;

        }
    }
}