using System.Text.RegularExpressions;

namespace AutoNotionTube.Core.Extensions
{
    public static class YoutubeVideoExtensions
    {
        public static int GetApproximateCaptionWaitTime(this int seconds, double sizeMb)
        {
            const double referenceSizeMB = 10.0;
            const double referenceDurationSec = 10.0;
            const double referenceWaitTimeSec = 300.0;

            const double approximationRatio = (referenceSizeMB * referenceDurationSec) / referenceWaitTimeSec;

            double approximateWaitTimeSec = (sizeMb * seconds) / approximationRatio;

            var result = (int)Math.Round(approximateWaitTimeSec);

            //TODO: Remove this hack
            // return result < 300 ? 300 : result;
            return 1;
        }
        
        public static string GetVideoYoutubeUrl(this string videoId)
        {
            return $"https://www.youtube.com/watch?v={videoId}";
        }

        public static string GetVideoEmbedIframe(this string videoId, int width = 560, int height = 315)
        {
            return
                $"<iframe width=\"{width}\" height=\"{height}\" src=\"https://www.youtube.com/embed/{videoId}\" title=\"YouTube video player\" frameborder=\"0\" allow=\"accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share\" allowfullscreen></iframe>";
        }

        public static string RemoveTimestamps(this string caption)
        {
            string pattern = @"\d:\d\d:\d\d\.\d\d\d,\d:\d\d:\d\d\.\d\d\d";
            string noTimestamps = Regex.Replace(caption, pattern, "").Trim();
            return noTimestamps;
        }

        public static string RemoveExtraNewlines(this string text)
        {
            string pattern = @"\n{2,}";
            string result = Regex.Replace(text, pattern, "\n");
            return result;
        }
    }
}
