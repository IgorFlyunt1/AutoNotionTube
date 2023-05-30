using System.Text.RegularExpressions;

namespace AutoNotionTube.Infrastructure.Extensions
{
    public static class StringExtensions
    {
        public static string GetLastPathComponent(this string path)
        {
            char[] separators = { '\\', '/' };
            string[] parts = path.Split(separators);
            return parts[parts.Length - 1];
        }

        public static int GetVideoDurationInSeconds(this string path)
        {
            var ffProbe = new NReco.VideoInfo.FFProbe();
            var videoInfo = ffProbe.GetMediaInfo(path);
            return (int)videoInfo.Duration.TotalSeconds;
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
