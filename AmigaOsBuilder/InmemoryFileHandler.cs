using System;
using System.Collections.Generic;
using System.Text;

namespace AmigaOsBuilder
{
    internal class InmemoryFileHandler : IFileHandler
    {
        private string _content;
        private Attributes _attributes;
        private DateTime _lastWriteTime;

        public InmemoryFileHandler(string path, string content, Attributes attributes)
        {
            OutputBasePath = path;
            _content = content;
            _attributes = attributes;
            _lastWriteTime = DateTime.Now;
        }

        public string OutputBasePath { get; set; }

        public void CreateBasePaths(AliasService aliasService)
        {
            throw new NotImplementedException();
        }

        public void DirectoryCreateDirectory(string path)
        {
            throw new NotImplementedException();
        }

        public void DirectoryDelete(string path, bool recursive)
        {
            throw new NotImplementedException();
        }

        public bool DirectoryExists(string path)
        {
            throw new NotImplementedException();
        }

        public IList<string> DirectoryGetDirectories(string path)
        {
            throw new NotImplementedException();
        }

        public IList<string> DirectoryGetFiles(string path)
        {
            throw new NotImplementedException();
        }

        public IList<string> DirectoryGetFileSystemEntriesRecursive(string path)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
        }

        public void FileCopy(IFileHandler sourceFileHandler, string syncSourcePath, string path)
        {
            throw new NotImplementedException();
        }

        public void FileCopyBack(string path, IFileHandler contentFileHandler, string contentPath)
        {
            throw new NotImplementedException();
        }

        public void FileDelete(string path)
        {
            throw new NotImplementedException();
        }

        public bool FileExists(string path)
        {
            if (path.ToLowerInvariant() != OutputBasePath.ToLowerInvariant())
            {
                return false;
            }
            return true;
        }

        public byte[] FileReadAllBytes(string path)
        {
            if (FileExists(path) == false)
            {
                throw new Exception("File {path} not found in InmemoryFileHandler");
            }
            return Encoding.UTF8.GetBytes(_content);
        }

        public string FileReadAllText(string path)
        {
            if (FileExists(path) == false)            
            {
                throw new Exception("File {path} not found in InmemoryFileHandler");
            }
            return _content;
        }

        //public void FileWriteAllText(string path, string content)
        //{
        //    throw new NotImplementedException();
        //}

        public (DateTime DateTime, Attributes Attributes) GetDate(string path)
        {
            if (FileExists(path) == false)
            {
                throw new Exception("File {path} not found in InmemoryFileHandler");
            }

            return (_lastWriteTime, _attributes);
        }

        public IFileInfo GetFileInfo(string path)
        {
            var fileInfo = new InmemoryFileInfo(
                _content, 
                exists: true, 
                lastWriteTime: _lastWriteTime, 
                attributes: _attributes, 
                length: _content.Length);
            return fileInfo;
        }

        public FileType GetFileType(string path)
        {
            throw new NotImplementedException();
        }
    }
}