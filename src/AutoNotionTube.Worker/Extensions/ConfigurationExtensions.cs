namespace AutoNotionTube.Worker.Extensions;

public static class ConfigurationExtensions
{
    public static IConfiguration CreateConfiguration()
    {
        const string appsettingsJson = "appsettings.json";
        return new ConfigurationBuilder()
            .AddJsonFile(appsettingsJson)
            .Build();
    }
}
