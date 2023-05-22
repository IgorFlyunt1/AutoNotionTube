using AutoNotionTube.Core.Interfaces;
using AutoNotionTube.Infrastructure.Repositories;
using AutoNotionTube.Infrastructure.Services;
using AutoNotionTube.Infrastructure.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AutoNotionTube.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<YoutubeSettings>(configuration.GetSection(nameof(YoutubeSettings)));
        services.Configure<VideoFilesDirectorySettings>(configuration.GetSection(nameof(VideoFilesDirectorySettings)));
        services.AddSingleton<IYoutubeService, YoutubeService>();
        services.AddScoped<IVideoRepository, LocalVideoRepository>();
        return services;
    }
}
