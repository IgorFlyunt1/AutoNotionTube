using AutoNotionTube.Core.Interfaces;
using Google.Apis.YouTube.v3;

namespace AutoNotionTube.Infrastructure.Services;

public class YoutubeService : IYoutubeService
{
    private readonly YouTubeService? _youTubeService;

    public YoutubeService(YouTubeService? youTubeService)
    {
        _youTubeService = youTubeService;
    }

    public Task<YouTubeService?> GetService(CancellationToken cancellationToken)
    {
        return Task.FromResult(_youTubeService);
    }

    public async Task<bool> VideoHasCaptions(string videoId, CancellationToken cancellationToken)
    {
        var captionsListRequest = _youTubeService!.Captions.List("snippet", videoId);

        var captionsListResponse = await captionsListRequest.ExecuteAsync(cancellationToken);

        return captionsListResponse.Items.Count > 0;
    }

    public async Task<string> GetCaptions(string videoId, CancellationToken cancellationToken)
    {
        var captionsListRequest = _youTubeService!.Captions.List("snippet", videoId);
        var captionsListResponse = await captionsListRequest.ExecuteAsync(cancellationToken);
        var captionId = captionsListResponse.Items[0].Id;

        var captionDownloadRequest = _youTubeService.Captions.Download(captionId);

        var caption = await captionDownloadRequest.ExecuteAsync(cancellationToken);

        return caption;
    }
}
