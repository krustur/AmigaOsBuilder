using System;
using System.Linq;

namespace AmigaOsBuilder
{
    public interface IPathService
    {
        string Combine(string path0, string path1);
        string Combine(string path0, string path1, string path2);
        string GetFileName(string path);
        string GetDirectoryName(string path);
    }

    public class PathService : IPathService
    {
        public string Combine(string path0, string path1)
        {
            var path = System.IO.Path.Combine(path0, path1);
            return path;
        }

        public string Combine(string path0, string path1, string path2)
        {
            var path = System.IO.Path.Combine(path0, path1, path2);
            return path;
        }

        public string GetFileName(string path)
        {
            var fileName = System.IO.Path.GetFileName(path);
            return fileName;
        }

        public string GetDirectoryName(string path)
        {
            var dirName = System.IO.Path.GetDirectoryName(path);
            return dirName;
        }
    }
}