using Google.Apis.YouTube.v3.Data;

namespace AutoNotionTube.Core.DTOs;

public class YoutubeApiVideo
{
    public VideoSnippet Snippet { get; set; }
    public VideoStatus Status { get; set; }
}
