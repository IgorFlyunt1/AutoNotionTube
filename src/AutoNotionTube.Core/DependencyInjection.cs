using AutoNotionTube.Core.Interfaces;
using AutoNotionTube.Core.Services;
using AutoNotionTube.Core.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AutoNotionTube.Core;

public static class DependencyInjection
{
    public static IServiceCollection AddCore(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<YoutubeSettings>(configuration.GetSection(nameof(YoutubeSettings)));
        services.AddSingleton<IYoutubeService, YoutubeService>();
        return services;
    }
}
