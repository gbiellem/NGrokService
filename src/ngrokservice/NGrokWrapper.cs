using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

public class NGrokWrapper : IHostedService, IHostApplicationLifetime
{
    Process p;
    readonly string exePath;
    readonly string configPath;

    public NGrokWrapper(IConfiguration configuration)
    {
        exePath = configuration["NGrok:ExePath"];
        configPath = configuration["NGrok:ConfigPath"];
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        ValidateConfig();
        Log.Information("Starting Ngrok.exe");
        var psi = new ProcessStartInfo
        {
            WindowStyle = ProcessWindowStyle.Hidden,
            CreateNoWindow = true,
            Arguments = $"start --all --config {configPath}",
            FileName = exePath
        };

        p = new Process {StartInfo = psi, EnableRaisingEvents = true};
        p.Exited += (sender, args) =>
        {
            if (cancellationToken.IsCancellationRequested)
            {
                if (!ApplicationStopping.IsCancellationRequested && !ApplicationStopped.IsCancellationRequested)
                {
                    throw new Exception($"NGrok has terminated prematurely: {p.ExitCode}");
                }
            }
        };
        p.ErrorDataReceived += (sender, args) =>
        {
            Log.Error(args.Data);
        };
    
        if (!p.Start())
        {
            throw new Exception($"NGrok failed to start: {p.ExitCode}");
        }

        Log.Information($"NGrok running as Pid: {p.Id}");
        
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        p.Kill(true);
        return Task.CompletedTask;
    }

    void ValidateConfig()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(exePath))
        {
            errors.Add("Configuration NGrok:ExePath did not provide a valid path");
        }
        else
        {
            try
            {
                if (!File.Exists(exePath))
                {
                    errors.Add("Configuration NGrok:ExePath does not point to NGrok.exe");
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Failed to find NGrok.exe at {exePath}. Exception Message: {ex.Message}");
            }
        }

        if (string.IsNullOrWhiteSpace(configPath))
        {
            errors.Add("Configuration NGrok:ConfigPath did not provide a valid path");
        }
        else
        {
            try
            {
                if (!File.Exists(configPath))
                {
                    errors.Add("Configuration NGrok:ConfigPath does not point to a file");
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Failed to find file at {configPath}. Exception Message: {ex.Message}");
            }
        }

        if (errors.Count > 0)
        {
            foreach (var error in errors)
            {
                Log.Error(error);

            }
            throw new Exception("Failed to Start ngrok.exe due to configuration errors");
        }
    }


    public void StopApplication()
    {
        throw new NotImplementedException();
    }

    public CancellationToken ApplicationStarted { get; }
    public CancellationToken ApplicationStopping { get; }
    public CancellationToken ApplicationStopped { get; }
}
