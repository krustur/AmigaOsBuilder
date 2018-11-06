using System;
using System.Collections.Generic;

namespace AmigaOsBuilder
{
    public interface IFileHandler : IDisposable
    {
        string OutputBasePath { get; }

        void CreateBasePaths(AliasService aliasService);

        bool FileExists(string path);
        string FileReadAllText(string path);
        void FileCopy(IFileHandler sourceFileHandler, string syncSourcePath, string path);
        void FileCopyBack(string path, IFileHandler contentFileHandler, string contentPath);
        void FileDelete(string path);

        bool DirectoryExists(string path);
        void DirectoryCreateDirectory(string path);
        void DirectoryDelete(string path, bool recursive);
        
        FileType GetFileType(string path);
        IFileInfo GetFileInfo(string path);
        (DateTime DateTime, Attributes Attributes) GetDate(string path);

        byte[] FileReadAllBytes(string path);

        IList<string> DirectoryGetFileSystemEntriesRecursive(string path);
        //IList<string> DirectoryGetDirectories(string path);
        //IList<string> DirectoryGetFiles(string path);

    }
}