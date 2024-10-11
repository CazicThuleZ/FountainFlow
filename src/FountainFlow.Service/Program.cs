using FountainFlow.Service.Config;
using FountainFlow.Service.Interfaces;
using FountainFlow.Service.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Quartz;
using Serilog;

namespace FountainFlow.Service;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseWindowsService()
            .UseSystemd()
            .UseSerilog((hostingContext, loggerConfiguration) => loggerConfiguration
                .ReadFrom.Configuration(hostingContext.Configuration)
                .Enrich.FromLogContext())
            .ConfigureServices((hostContext, services) =>
            {
                //services.AddHostedService<Worker>();

                services.Configure<GlobalSettings>(
                    hostContext.Configuration.GetSection("GlobalSettings"));

                var jobSchedules = hostContext.Configuration.GetSection("ScheduleSettings").Get<ScheduleSettings>();

                if (jobSchedules == null)
                    throw new Exception("Failed to load ScheduleSettings from configuration.");

                services.Configure<IntakeJobSettings>(hostContext.Configuration.GetSection("IntakeJobSettings"));

                services.AddSingleton(resolver =>
                    resolver.GetRequiredService<IOptions<GlobalSettings>>().Value);

                services.AddSingleton<ISemanticKernelServiceFactory, SemanticKernelServiceFactory>();

                services.AddTransient<ISemanticKernelService>(provider =>
                {
                    var factory = provider.GetRequiredService<ISemanticKernelServiceFactory>();
                    return factory.ClassifyFountainTextAsync();
                });
                services.AddTransient<ISemanticKernelService>(provider =>
                {
                    var factory = provider.GetRequiredService<ISemanticKernelServiceFactory>();
                    return factory.DeriveThemeAsync();
                });

                services.AddQuartz(q =>
                {
                    q.AddJobAndTrigger<ScreenplayIntakeJob>(jobSchedules.ScreenplayIntakeJob);
                });

                services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
            });
}