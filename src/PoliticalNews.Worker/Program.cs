using PoliticalNews.Application.DependencyInjection;
using PoliticalNews.Infrastructure.DependencyInjection;
using PoliticalNews.Worker.HostedServices;

// `dotnet run --project src/PoliticalNews.Worker -- --init-db` creates every MongoDB
// collection/index (see MongoIndexInitializerHostedService) and exits - a repeatable, idempotent
// database setup script with no separate scheduler/crawl started.
var initDbOnly = args.Contains("--init-db", StringComparer.OrdinalIgnoreCase);

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services.Configure<HostOptions>(options =>
{
    // Graceful shutdown: give an in-flight crawl run time to finish before the process exits.
    options.ShutdownTimeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

if (!initDbOnly)
{
    builder.Services.AddHostedService<CrawlerBackgroundService>();
}

var host = builder.Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();

if (initDbOnly)
{
    logger.LogInformation("--init-db: creating MongoDB collections/indexes only");
    foreach (var hostedService in host.Services.GetServices<IHostedService>())
    {
        await hostedService.StartAsync(CancellationToken.None);
    }
    logger.LogInformation("--init-db: done");
    return;
}

logger.LogInformation("PoliticalNews.Worker application started");

await host.RunAsync();
