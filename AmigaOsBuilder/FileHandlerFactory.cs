using System;
using Serilog.Core;
using System.Collections.Generic;

namespace AmigaOsBuilder
{
    public class FileHandlerFactory
    {
        private static IDictionary<string, IFileHandler> _customFileHandlers { get; } = new Dictionary<string, IFileHandler>();

        public static IFileHandler Create(Logger logger, string outputBasePath)
        {
            if (outputBasePath == null)
            {
                return new NullFileHandler(outputBasePath);
            }

            if (IsLhaFile(outputBasePath))
            {
                return new LhaFileHandler(logger, outputBasePath);
            }

            var customFileHandler = GetCustomFileHandler(outputBasePath);
            if (customFileHandler != null)
            {
                return customFileHandler;
            }
            //if (IsReadmeFile(outputBasePath))
            //{
            //    if (ReadmeFileHandler == null)
            //    {
            //        throw new Exception("ReadmeFileHandler is requested, but not yet set");
            //    }
            //    return ReadmeFileHandler;
            //}


            return new FileSystemFileHandler(logger, outputBasePath);
        }


        private static IFileHandler GetCustomFileHandler(string outputBasePath)
        {
            foreach (var pair in _customFileHandlers)
            {
                var isPath = outputBasePath.ToLowerInvariant() == pair.Key.ToLowerInvariant();
                if (isPath)
                {
                    return pair.Value;
                }
            }
            return null;
        }
        //private static bool IsReadmeFile(string outputBasePath)
        //{
        //    var isReadmeFile = outputBasePath.ToLowerInvariant() == "krustwb3.readme.txt";
        //    return isReadmeFile;
        //}

        private static bool IsLhaFile(string outputBasePath)
        {
            var isLhaFile = outputBasePath.ToLowerInvariant().EndsWith(".lha") || outputBasePath.ToLowerInvariant().EndsWith(".lzh");
            return isLhaFile;
        }

        internal static void AddCustomFileHandler(string path, IFileHandler inmemoryFileHandler)
        {
            if (_customFileHandlers.ContainsKey(path))
            {
                _customFileHandlers.Remove(path);
            }
            _customFileHandlers.Add(path, inmemoryFileHandler);
        }
    }
}