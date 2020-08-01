using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

static class Program
{
    static async Task Main()
    {
        AppSetup.Initialize();

        using var host = Host.CreateDefaultBuilder()
            .ConfigureServices((hostContext, services) =>
            {
                services.Configure<ConsoleLifetimeOptions>(_ => { _.SuppressStatusMessages = true; });
                services.AddLogging(_ => { _.AddSerilog(dispose: true); });
            })
            .UseWindowsService()
            .UseSerilog()
            .Build();

        host.Services.GetService<IHostApplicationLifetime>().ApplicationStarted.Register(() =>
        {

        });

        await host.StartAsync();
        await host.WaitForShutdownAsync();
    }
}