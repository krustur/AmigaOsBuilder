using System;
using System.Collections.Generic;

namespace AmigaOsBuilder
{
    public interface IFileHandler : IDisposable
    {
        void CreateBasePaths(AliasService aliasService);
        bool FileExists(string path);
        string FileReadAllText(string path);
        void FileWriteAllText(string path, string content);
        void FileCopy(IFileHandler sourceFileHandler, string syncSourcePath, string path);
        void FileCopyBack(string path, IFileHandler contentFileHandler, string contentPath);
        void FileDelete(string path);
        bool DirectoryExists(string path);
        void DirectoryCreateDirectory(string path);
        void DirectoryDelete(string path, bool recursive);
        IList<string> DirectoryGetFileSystemEntriesRecursive(string path);
        //string GetSubPath(string fullPath);
        FileType GetFileType(string path);
        IFileInfo GetFileInfo(string path);
        IList<string> DirectoryGetDirectories(string path);
        string OutputBasePath { get; }
        byte[] FileReadAllBytes(string path);
        IList<string> DirectoryGetFiles(string path);
        DateTime GetDate(string path);
    }
}