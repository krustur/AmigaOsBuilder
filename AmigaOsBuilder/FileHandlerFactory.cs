using Serilog.Core;

namespace AmigaOsBuilder
{
    public class FileHandlerFactory
    {
        public static IFileHandler Create(Logger logger, string outputBasePath)
        {
            if (IsLhaFile(outputBasePath))
            {
                return new LhaFileHandler(logger, outputBasePath);

            }
            return new FolderOutputHandler(logger, outputBasePath);
        }

        private static bool IsLhaFile(string outputBasePath)
        {
            return outputBasePath.ToLowerInvariant().EndsWith(".lha") || outputBasePath.ToLowerInvariant().EndsWith(".lzh");
        }
    }
}