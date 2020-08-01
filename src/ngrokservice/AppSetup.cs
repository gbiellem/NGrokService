using System;
using System.Globalization;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Hosting.WindowsServices;
using Serilog;

static class AppSetup
{
    const string AustralianCulture = "en-AU";

    public static void Initialize()
    {
        Culture();
        Logging();
    }

    static void Culture()
    {
        if (Thread.CurrentThread.CurrentCulture.Name.Equals(AustralianCulture, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }
        var culture = CultureInfo.CreateSpecificCulture(AustralianCulture);
        Thread.CurrentThread.CurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
    }

    static void Logging()
    {
        var logger = new LoggerConfiguration()
            .Enrich.FromLogContext();

        if (!WindowsServiceHelpers.IsWindowsService())
        {
            Console.Title = AppDomain.CurrentDomain.FriendlyName;
            logger.WriteTo.Console();
        }

        var rootDirectory = WindowsServiceHelpers.IsWindowsService()
            ? Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)
            : Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData); //Assume interactive running is for debugging, so don't muddy the logs

        var directory = Path.Combine(rootDirectory, "logs", AppDomain.CurrentDomain.FriendlyName);

        // Blow up if we can't write to the log directory    
        Directory.CreateDirectory(directory);
        var temporaryFile = $"{directory}\\{DateTime.Now.Ticks}.tmp";
        File.WriteAllText(temporaryFile, " ");
        File.Delete(temporaryFile);

        logger.WriteTo.File(
                $@"{directory}\log.txt",
                rollingInterval: RollingInterval.Day,
                rollOnFileSizeLimit: true,
                fileSizeLimitBytes: 1000000, //1mb
                retainedFileCountLimit: 5,
                formatProvider: new CultureInfo("en-AU"));

        Log.Logger = logger.CreateLogger();
        Log.Information($"Logging to {directory}");
    }
}
