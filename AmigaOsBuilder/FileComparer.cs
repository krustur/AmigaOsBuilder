using System;
using System.IO;

namespace AmigaOsBuilder
{
    public class FileComparer
    {
        public static bool FilesContentsAreEqual(IFileInfo fileInfo1, IFileInfo fileInfo2)
        {
            bool result;

            if (fileInfo1.Exists != fileInfo2.Exists)
            {
                return false;
            }

            if (fileInfo1.Length != fileInfo2.Length)
            {
                result = false;
            }
            else
            {
                using (var file1 = fileInfo1.OpenRead())
                {
                    using (var file2 = fileInfo2.OpenRead())
                    {
                        result = StreamsContentsAreEqual(file1, file2);
                    }
                }
            }

            return result;
        }

        private static bool StreamsContentsAreEqual(IStream stream1, IStream stream2)
        {
            const int bufferSize = 1024 * sizeof(Int64);
            var buffer1 = new byte[bufferSize];
            var buffer2 = new byte[bufferSize];

            while (true)
            {
                int count1 = stream1.Read(buffer1, 0, bufferSize);
                int count2 = stream2.Read(buffer2, 0, bufferSize);

                if (count1 != count2)
                {
                    return false;
                }

                if (count1 == 0)
                {
                    return true;
                }

                //int iterations = (int)Math.Ceiling((double)count1 / sizeof(Int64));
                //for (int i = 0; i < iterations; i++)
                //{
                //    if (BitConverter.ToInt64(buffer1, i * sizeof(Int64)) != BitConverter.ToInt64(buffer2, i * sizeof(Int64)))
                //    {
                //        return false;
                //    }
                //}
                //int iterations = (int)Math.Ceiling((double)count1 / sizeof(Int64));
                for (int i = 0; i < count1; i++)
                {
                    if (buffer1[i] != buffer2[i])
                    {
                        return false;
                    }
                }
            }
        }
    }
}