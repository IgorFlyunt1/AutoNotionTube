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
        services.AddSingleton<IYoutubeService, YoutubeService>();
        services.AddScoped<IVideoRepository, LocalVideoRepository>();
        
        // AddYoutubeService(services);
        
        return services;
    }

    // private static void AddYoutubeService(IServiceCollection services) =>
    //     services.AddSingleton<YouTubeService>(sp =>
    //     {
    //         var youtubeSettings = sp.GetRequiredService<IOptions<YoutubeSettings>>().Value;
    //         YouTubeService youtubeService;
    //
    //         using (var stream = new FileStream(youtubeSettings.ClientSecretsFilePath, FileMode.Open, FileAccess.Read))
    //         {
    //             var secrets = GoogleClientSecrets.FromStream(stream).Result;
    //
    //             youtubeService = new YouTubeService(new BaseClientService.Initializer()
    //             {
    //                 HttpClientInitializer = GoogleWebAuthorizationBroker.AuthorizeAsync(
    //                     secrets.Secrets,
    //                     new[] { YouTubeService.Scope.YoutubeUpload },
    //                     "user",
    //                     CancellationToken.None
    //                 ).Result,
    //                 ApplicationName = typeof(YouTubeService).ToString()
    //             });
    //         }
    //
    //         return youtubeService;
    //     });
}
