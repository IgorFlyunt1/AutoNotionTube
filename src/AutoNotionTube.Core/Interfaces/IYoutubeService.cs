using Google.Apis.YouTube.v3;

namespace AutoNotionTube.Core.Interfaces;

public interface IYoutubeService
{
    Task<YouTubeService?> GetService(CancellationToken cancellationToken);
    
    Task<bool> VideoHasCaptions(string videoId, CancellationToken cancellationToken);
    Task<string> GetCaptions(string videoId, CancellationToken cancellationToken);
}
