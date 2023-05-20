using Google.Apis.YouTube.v3;

namespace AutoNotionTube.Core.Interfaces;

public interface IYoutubeService
{
    Task<YouTubeService> CreateService(CancellationToken cancellationToken);
}
