using FountainFlow.UI;
using FountainFlowUI.Interfaces;
using FountainFlowUI.Repository;
using Serilog;
using Serilog.Events;
using Serilog.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("Logs/fountainflow-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30)
    .CreateLogger();

try
{
    Log.Information("Starting web application");
    builder.Host.UseSerilog();
    builder.Services.AddControllersWithViews();

    builder.Services.AddHttpClient<IArchetypesRepository, ArchetypesRepository>(client =>
    {
        client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]);
    });

    builder.Services.AddScoped<IArchetypesRepository, ArchetypesRepository>();

    // Configure CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowSpecificOrigin",
            builder =>
            {
                builder.WithOrigins("http://localhost:5087")
                       .AllowAnyHeader()
                       .AllowAnyMethod();
            });
    });

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseRouting();

    app.UseAuthorization();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Home}");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}



