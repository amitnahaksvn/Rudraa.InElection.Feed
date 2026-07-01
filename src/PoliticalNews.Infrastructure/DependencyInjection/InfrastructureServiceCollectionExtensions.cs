using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using PoliticalNews.Application.Abstractions;
using PoliticalNews.Application.Options;
using PoliticalNews.Infrastructure.Mongo;
using PoliticalNews.Infrastructure.Persistence;
using PoliticalNews.Infrastructure.RssProviders;

namespace PoliticalNews.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    /// <summary>
    /// Registers Mongo, the repository layer, and every <see cref="IRssProvider"/>. Adding a new
    /// provider in a future phase is one line here plus one appsettings.json config block.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        MongoClassMapConfigurator.Configure();

        services
            .AddOptions<MongoDbOptions>()
            .Bind(configuration.GetSection(MongoDbOptions.SectionName))
            // Aspire (and the ASP.NET Core convention in general) injects resource connection
            // strings under ConnectionStrings:<name> - e.g. AppHost.cs's "mongodb" resource
            // becomes ConnectionStrings__mongodb. When present it wins over MongoDb:ConnectionString,
            // so the same code runs unchanged whether launched via the Aspire AppHost, plain
            // `dotnet run`, or docker-compose.
            .PostConfigure(options =>
            {
                var aspireConnectionString = configuration.GetConnectionString("mongodb");
                if (!string.IsNullOrWhiteSpace(aspireConnectionString))
                {
                    options.ConnectionString = aspireConnectionString;
                }
            })
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<MongoDbContext>();
        services.AddSingleton<IMongoClient>(sp => sp.GetRequiredService<MongoDbContext>().Client);

        services.AddSingleton<INewsArticleRepository, NewsArticleRepository>();
        services.AddSingleton<ICrawlHistoryRepository, CrawlHistoryRepository>();
        services.AddSingleton<ICrawlLockRepository, CrawlLockRepository>();
        services.AddSingleton<IRssRawResponseRepository, RssRawResponseRepository>();

        services.AddHttpClient(AajTakRssProvider.ClientName, (sp, client) =>
        {
            client.Timeout = sp.GetRequiredService<IOptions<NewsCrawlerOptions>>().Value.FeedTimeout;
            client.DefaultRequestHeaders.UserAgent.ParseAdd(
                "Mozilla/5.0 (compatible; PoliticalNewsCrawler/1.0; +https://example.com/bot)");
        });
        services.AddSingleton<IRssProvider, AajTakRssProvider>();

        services.AddHostedService<MongoIndexInitializerHostedService>();

        return services;
    }
}
