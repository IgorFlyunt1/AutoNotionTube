using AutoNotionTube.Core.Interfaces;
using AutoNotionTube.Infrastructure.Repositories;
using AutoNotionTube.Infrastructure.Services;
using AutoNotionTube.Infrastructure.Settings;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace AutoNotionTube.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<YoutubeSettings>(configuration.GetSection(nameof(YoutubeSettings)));
        services.Configure<VideoFilesDirectorySettings>(configuration.GetSection(nameof(VideoFilesDirectorySettings)));
        services.Configure<OpenApiSettings>(configuration.GetSection(nameof(OpenApiSettings)));
        services.Configure<NotionSettings>(configuration.GetSection(nameof(NotionSettings)));
        services.AddSingleton<IYoutubeService, YoutubeService>();
        services.AddScoped<IVideoRepository, LocalVideoRepository>();
        services.AddScoped<IOpenApiService, OpenApiService>();
        services.AddScoped<INotionService, NotionService>();

        services.AddHttpClient();

        AddYoutubeService(services);

        return services;
    }

    private static void AddYoutubeService(IServiceCollection services) =>
        services.AddScoped<YouTubeService>(provider =>
        {
            var youtubeSettings = provider.GetRequiredService<IOptions<YoutubeSettings>>().Value;

            using var stream = new FileStream(youtubeSettings.ClientSecretsFilePath, FileMode.Open, FileAccess.Read);
            var secrets = GoogleClientSecrets.FromStreamAsync(stream).GetAwaiter().GetResult();

            var youTubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    secrets.Secrets,
                    new[] { YouTubeService.Scope.YoutubeUpload, YouTubeService.Scope.YoutubeForceSsl },
                    "user",
                    CancellationToken.None
                ).GetAwaiter().GetResult(),
                ApplicationName = typeof(YouTubeService).ToString()
            });

            return youTubeService;
        });
}
