using AutoNotionTube.Core;
using AutoNotionTube.Domain;
using AutoNotionTube.Infrastructure;
using AutoNotionTube.Worker;
using Serilog;
using AutoNotionTube.Worker.Extensions;
using MediatR;
using ConfigurationExtensions = AutoNotionTube.Worker.Extensions.ConfigurationExtensions;

var configuration = ConfigurationExtensions.CreateConfiguration();
LoggingAndTelemetryExtension.AddSerilog(configuration);

try
{
    Log.Information("Starting up the AutoNotionTube service");

    IHost host = Host.CreateDefaultBuilder(args)
        .ConfigureServices(services =>
        {
            services.AddHostedService<Worker>();

            services.AddMediatR(
                typeof(Program),
                typeof(AutoNotionTube.Core.DependencyInjection),
                typeof(AutoNotionTube.Infrastructure.DependencyInjection));

            services.AddCore(configuration);
            services.AddDomain(configuration);
            services.AddInfrastructure(configuration);
        })
        .UseSerilog() // use Serilog as the logging framework
        .Build();

    host.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "The service encountered a termination error");
}
finally
{
    Log.CloseAndFlush();
}
