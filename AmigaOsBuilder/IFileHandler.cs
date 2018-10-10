using System.Collections.Generic;

namespace AmigaOsBuilder
{
    public interface IFileHandler
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
}