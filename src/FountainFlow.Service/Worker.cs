using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

public class Worker : BackgroundService
{
    private readonly Serilog.ILogger _logger;

    public Worker(Serilog.ILogger logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.Information("Worker running at: {Time}", DateTimeOffset.Now);
            await Task.Delay(1000, stoppingToken);
        }
    }
}