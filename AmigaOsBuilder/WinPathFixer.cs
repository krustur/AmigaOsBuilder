using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;

namespace AmigaOsBuilder
{
    public class WinPathFixer
    {
        private static IList<string> invalidFileNames = new List<string>
        {
            "CON", "PRN", "AUX", "NUL", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9",
        };

        private StringBuilder _stringBuilder = new StringBuilder();

        public string FixPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return path;
            }
            _stringBuilder.Length = 0;
            var segments = path.Split('\\');
            bool isFirstPart = true;
            foreach (var segment in segments)
            {
                if (segment == "/") continue;

                var partFileName = segment
                    .Replace('/', '\\');

                if (invalidFileNames.Contains(partFileName))
                {
                    partFileName = $"_amigafixed_{partFileName}";
                }
                else
                {
                    foreach (var invalidFileName in invalidFileNames)
                    {
                        if (partFileName.ToUpperInvariant().StartsWith(invalidFileName + '.'))
                        {
                            partFileName = $"_amigafixed_{partFileName}";
                            break;
                        }
                        else if (partFileName.ToUpperInvariant().StartsWith(invalidFileName + '\\'))
                        {
                            partFileName = $"_amigafixed_{partFileName}";
                            break;
                        }
                    }
                }
                if (isFirstPart == false)
                {
                    _stringBuilder.Append('\\');
                }
                isFirstPart = false;
                _stringBuilder.Append(partFileName);
            }
            return _stringBuilder.ToString();
        }


        public string FixPathUriProblems(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return path;
            }
            try
            {
                var uri = new Uri(/*"file://"+*/path);
                _stringBuilder.Length = 0;
                foreach (var segment in uri.Segments)
                {
                    if (segment == "/") continue;

                    var partFileName = segment
                        .Replace('/', '\\')
                        //.Replace("%20", " ")
                        ;

                    var partFileName2 = partFileName;
                    partFileName = HttpUtility.UrlDecode(partFileName);
                    if (partFileName2 != partFileName) { }

                    if (invalidFileNames.Contains(partFileName))
                    {
                        partFileName = $"_amigafixed_{partFileName}";
                    }
                    else
                    {
                        foreach (var invalidFileName in invalidFileNames)
                        {
                            if (partFileName.ToUpperInvariant().StartsWith(invalidFileName + '.'))
                            {
                                partFileName = $"_amigafixed_{partFileName}";
                                break;
                            }
                            else if (partFileName.ToUpperInvariant().StartsWith(invalidFileName + '\\'))
                            {
                                partFileName = $"_amigafixed_{partFileName}";
                                break;
                            }
                        }
                    }
                    _stringBuilder.Append(partFileName);
                }
            }
            catch (Exception e)
            {

            }
            return _stringBuilder.ToString();
        }

        public string DefixPath(string path)
        {
            var defixedPath = path.Replace("_amigafixed_", string.Empty);
            return defixedPath;
        }

    }
}