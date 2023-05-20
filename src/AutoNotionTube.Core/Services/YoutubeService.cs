using AutoNotionTube.Core.Constants;
using AutoNotionTube.Core.Interfaces;
using AutoNotionTube.Core.Settings;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Microsoft.Extensions.Options;

namespace AutoNotionTube.Core.Services;

public class YoutubeService : IYoutubeService
{
    private readonly YoutubeSettings _youtubeSettings;

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
                new[] { YouTubeService.Scope.YoutubeUpload },
                "user",
                cancellationToken
            ),
            ApplicationName = GetType().ToString()
        });

        return youtubeService;
    }
}
