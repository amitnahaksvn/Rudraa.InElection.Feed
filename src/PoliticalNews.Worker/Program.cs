using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.Configuration.Json;
using PoliticalNews.Application.DependencyInjection;
using PoliticalNews.Infrastructure.DependencyInjection;
using PoliticalNews.Worker.HostedServices;

// `dotnet run --project src/PoliticalNews.Worker -- --init-db` creates every MongoDB
// collection/index (see MongoIndexInitializerHostedService) and exits - a repeatable, idempotent
// database setup script with no separate scheduler/crawl started.
var initDbOnly = args.Contains("--init-db", StringComparer.OrdinalIgnoreCase);

var builder = Host.CreateApplicationBuilder(args);

// Shared with PoliticalNews.Web (see NewsCrawler.appsettings.json at the src/ root) so both
// processes read the exact same provider/feed/schedule config from one file, not a duplicated copy.
// AppContext.BaseDirectory (not ContentRootPath, which `dotnet run` sets to the project source
// directory) is what's consistent between `dotnet run` and a published/Docker deployment - the
// .csproj's linked Content item copies the file there under both build and publish. Inserted
// before the environment-variables source (rather than appended, which is CreateApplicationBuilder's
// default for a source added afterwards) so NewsCrawler__* env vars - e.g. from an AWS/Azure
// secret injected into the container's environment - can still override this file, not the reverse.
InsertNewsCrawlerConfigBeforeEnvironmentVariables(builder.Configuration);

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

static void InsertNewsCrawlerConfigBeforeEnvironmentVariables(IConfigurationBuilder configuration)
{
    var source = new JsonConfigurationSource
    {
        Path = Path.Combine(AppContext.BaseDirectory, "NewsCrawler.appsettings.json"),
        Optional = false,
        ReloadOnChange = true
    };
    source.ResolveFileProvider();

    var envVariablesIndex = configuration.Sources.ToList().FindIndex(s => s is EnvironmentVariablesConfigurationSource);
    if (envVariablesIndex < 0)
    {
        configuration.Add(source);
        return;
    }

    configuration.Sources.Insert(envVariablesIndex, source);
}
