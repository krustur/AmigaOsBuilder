using System;

namespace AmigaOsBuilder
{
    class ProgressBar
    {
        private static string _progressLastTitle = string.Empty;
        private static int _progressLastWidth = 0;

        public static void DrawProgressBar(string title, int i, int packagesCount)
        {
            if (_progressLastTitle != title)
            {
                _progressLastTitle = title;
                _progressLastWidth = 0;

            }
            var barWidth = 50;
            var maxWidth = (packagesCount - 1);
            if (maxWidth == 0)
            {
                maxWidth = 1;
            }
            var progressWidth = (i * barWidth) / maxWidth;

            if (progressWidth > _progressLastWidth)
            {
                _progressLastWidth = progressWidth;

                var barText = string.Format("{0}: [{1}{2}]\r",
                    title,
                    new string('#', progressWidth),
                    new string('-', barWidth - progressWidth));

                Console.Write(barText);
            }
        }

        public static void ClearProgressBar()
        {

            var barText = string.Format("{0}\r",
                new string(' ', 80));

            Console.Write(barText);
        }
    }
}