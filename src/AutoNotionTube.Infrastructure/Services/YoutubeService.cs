using AutoNotionTube.Core.Interfaces;
using AutoNotionTube.Infrastructure.Settings;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Microsoft.Extensions.Options;

namespace AutoNotionTube.Infrastructure.Services;

public class YoutubeService : IYoutubeService
{
    private readonly YoutubeSettings _youtubeSettings;
    private YouTubeService? _youTubeService;

    public YoutubeService(IOptions<YoutubeSettings> youtubeSettings)
    {
        _youtubeSettings = youtubeSettings.Value;
    }

    public async Task<YouTubeService> CreateService(CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(_youtubeSettings.ClientSecretsFilePath, FileMode.Open, FileAccess.Read);
        var secrets = await GoogleClientSecrets.FromStreamAsync(stream, cancellationToken);

        var youtubeService = new YouTubeService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                secrets.Secrets,
                new[] { YouTubeService.Scope.YoutubeUpload, YouTubeService.Scope.YoutubeForceSsl },
                "user",
                cancellationToken
            ),
            ApplicationName = GetType().ToString()
        });

        _youTubeService = youtubeService;
        return _youTubeService;
    }

    public async Task<bool> VideoHasCaptions(string videoId, CancellationToken cancellationToken)
    {
        var captionsListRequest = _youTubeService!.Captions.List("snippet", videoId);

        var captionsListResponse = await captionsListRequest.ExecuteAsync(cancellationToken);

        return captionsListResponse.Items.Count > 0;
    }

    public async Task<string> GetCaptions(string videoId, CancellationToken cancellationToken)
    {
        videoId = "7BSNKfCpVf8";
        var captionsListRequest = _youTubeService!.Captions.List("snippet", videoId);
        var captionsListResponse = await captionsListRequest.ExecuteAsync(cancellationToken);
        var captionId = captionsListResponse.Items[0].Id;

        var captionDownloadRequest = _youTubeService.Captions.Download(captionId);

        var caption = await captionDownloadRequest.ExecuteAsync(cancellationToken);

        return caption;
    }
}