using AutoNotionTube.Core;
using AutoNotionTube.Domain;
using AutoNotionTube.Infrastructure;
using AutoNotionTube.Worker;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
        
        services.AddDomain();
        services.AddCore();
        services.AddInfrastructure();
    })
    .Build();

host.Run();
