using System;
using Serilog.Core;

namespace AmigaOsBuilder
{
    public class FileHandlerFactory
    {
        public static IFileHandler ReadmeFileHandler { get; set; }

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

            if (IsReadmeFile(outputBasePath))
            {
                if (ReadmeFileHandler == null)
                {
                    throw new Exception("ReadmeFileHandler is requested, but not yet set");
                }
                return ReadmeFileHandler;
            }

            return new FileSystemFileHandler(logger, outputBasePath);
        }

        private static bool IsReadmeFile(string outputBasePath)
        {
            var isReadmeFile = outputBasePath.ToLowerInvariant() == "krustwb3.readme.txt";
            return isReadmeFile;
        }

        private static bool IsLhaFile(string outputBasePath)
        {
            var isLhaFile = outputBasePath.ToLowerInvariant().EndsWith(".lha") || outputBasePath.ToLowerInvariant().EndsWith(".lzh");
            return isLhaFile;
        }
    }
}