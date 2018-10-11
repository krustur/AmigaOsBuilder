using Serilog.Core;

namespace AmigaOsBuilder
{
    public class FileHandlerFactory
    {
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
            return new FileSystemFileHandler(logger, outputBasePath);
        }

        private static bool IsLhaFile(string outputBasePath)
        {
            return outputBasePath.ToLowerInvariant().EndsWith(".lha") || outputBasePath.ToLowerInvariant().EndsWith(".lzh");
        }
    }
}