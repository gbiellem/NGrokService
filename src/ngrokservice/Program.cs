using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

AppSetup.Initialize();

using var host = Host.CreateDefaultBuilder()
    .ConfigureServices((hostContext, services) =>
    {
        services.Configure<ConsoleLifetimeOptions>(_ => { _.SuppressStatusMessages = true; });
        services.AddHostedService<NGrokWrapper>();
        services.AddLogging(_ => { _.AddSerilog(dispose: true); });
    })
    .UseWindowsService()
    .UseSerilog()
    .Build();

await host.RunAsync();