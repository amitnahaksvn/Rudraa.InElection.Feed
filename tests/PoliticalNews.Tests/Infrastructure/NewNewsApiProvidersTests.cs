using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Application.Options;
using Domain.Enums;
using Infrastructure.NewsApiProviders;
using PoliticalNews.Tests.TestSupport;

namespace PoliticalNews.Tests.Infrastructure;

/// <summary>
/// Coverage for the second batch of news-API providers - especially Event Registry, the one that
/// doesn't fit <see cref="BaseNewsApiProvider"/>'s plain GET model (POST+JSON body instead), plus
/// GDELT's keyless auth and the <see cref="ArticleSourceType.Api"/> stamping every JSON-API
/// provider gets centrally.
/// </summary>
public class NewNewsApiProvidersTests
{
    private static IConfiguration ConfigWithKey(string providerName, string key) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { [$"NewsApiKeys:{providerName}"] = key })
            .Build();

    [Fact]
    public async Task GdeltProvider_ParsesArticles_WithNoApiKeyRequired()
    {
        const string url = "https://api.gdeltproject.org/api/v2/doc/doc?query=India&mode=artlist&format=json";
        const string json = """
            {
              "articles": [
                {
                  "url": "https://example.com/gdelt-1",
                  "title": "GDELT Headline",
                  "seendate": "20260701T101500Z",
                  "socialimage": "https://example.com/gdelt.jpg",
                  "domain": "example.com",
                  "language": "English",
                  "sourcecountry": "India"
                }
              ]
            }
            """;

        var options = new NewsApiProviderOptions
        {
            Name = GdeltProvider.ProviderName,
            BaseUrl = "https://api.gdeltproject.org/api/v2/doc",
            AuthType = ApiAuthType.None,
            TimeoutSeconds = 30,
            Endpoints =
            [
                new NewsApiEndpointOptions
                {
                    Name = "DocSearch",
                    Endpoint = "doc",
                    QueryParameters = new Dictionary<string, string> { ["query"] = "India", ["mode"] = "artlist", ["format"] = "json" },
                    Category = "Politics",
                    Language = "en",
                    Enabled = true
                }
            ]
        };

        // No key configured at all - GDELT must still succeed since AuthType.None skips that check entirely.
        var provider = new GdeltProvider(
            new StubHttpClientFactory(new StubHttpMessageHandler(new Dictionary<string, string> { [url] = json })),
            new ConfigurationBuilder().Build(),
            NullLogger<GdeltProvider>.Instance);

        var results = await provider.FetchAllEndpointsAsync(options, CancellationToken.None);

        var result = Assert.Single(results);
        Assert.True(result.Success);
        var article = Assert.Single(result.Articles);
        Assert.Equal("GDELT Headline", article.Title);
        Assert.Equal(new DateTimeOffset(2026, 7, 1, 10, 15, 0, TimeSpan.Zero), article.PublishedAt);
        Assert.Equal(ArticleSourceType.Api, article.SourceType);
    }

    [Fact]
    public async Task GuardianProvider_ParsesNestedFieldsObject()
    {
        const string url = "https://content.guardianapis.com/search?q=India&api-key=test-key";
        const string json = """
            {
              "response": {
                "status": "ok",
                "total": 1,
                "results": [
                  {
                    "id": "world/2026/jul/01/example",
                    "sectionName": "World news",
                    "webTitle": "Guardian Headline",
                    "webUrl": "https://example.com/guardian-1",
                    "webPublicationDate": "2026-07-01T09:00:00Z",
                    "fields": {
                      "trailText": "Guardian summary",
                      "byline": "Jane Reporter",
                      "thumbnail": "https://example.com/guardian.jpg"
                    }
                  }
                ]
              }
            }
            """;

        var options = new NewsApiProviderOptions
        {
            Name = GuardianProvider.ProviderName,
            BaseUrl = "https://content.guardianapis.com",
            AuthType = ApiAuthType.QueryParameter,
            AuthParamName = "api-key",
            TimeoutSeconds = 30,
            Endpoints =
            [
                new NewsApiEndpointOptions
                {
                    Name = "Search",
                    Endpoint = "search",
                    QueryParameters = new Dictionary<string, string> { ["q"] = "India" },
                    Category = "Politics",
                    Language = "en",
                    Enabled = true
                }
            ]
        };

        var provider = new GuardianProvider(
            new StubHttpClientFactory(new StubHttpMessageHandler(new Dictionary<string, string> { [url] = json })),
            ConfigWithKey(GuardianProvider.ProviderName, "test-key"),
            NullLogger<GuardianProvider>.Instance);

        var results = await provider.FetchAllEndpointsAsync(options, CancellationToken.None);

        var result = Assert.Single(results);
        Assert.True(result.Success);
        var article = Assert.Single(result.Articles);
        Assert.Equal("Guardian Headline", article.Title);
        Assert.Equal("Guardian summary", article.Summary);
        Assert.Equal("Jane Reporter", article.Author);
        Assert.Equal("https://example.com/guardian.jpg", article.ImageUrl);
        Assert.Equal("World news", article.Category);
    }

    [Fact]
    public async Task EventRegistryProvider_PostsJsonBody_AndParsesNestedResultsWrapper()
    {
        const string url = "https://eventregistry.org/api/v1/article/getArticles";
        const string json = """
            {
              "articles": {
                "results": [
                  {
                    "uri": "1234567890",
                    "title": "Event Registry Headline",
                    "body": "Full article body text that is reasonably long.",
                    "url": "https://example.com/eventregistry-1",
                    "image": "https://example.com/er.jpg",
                    "lang": "eng",
                    "dateTimePub": "2026-07-01T08:00:00Z",
                    "source": { "uri": "example.com", "title": "Example Source" },
                    "authors": [ { "name": "John Author" } ]
                  }
                ],
                "totalResults": 1
              }
            }
            """;

        var options = new NewsApiProviderOptions
        {
            Name = EventRegistryProvider.ProviderName,
            BaseUrl = "https://eventregistry.org/api/v1/article/getArticles",
            TimeoutSeconds = 30,
            Endpoints =
            [
                new NewsApiEndpointOptions
                {
                    Name = "GetArticles",
                    Endpoint = "",
                    QueryParameters = new Dictionary<string, string> { ["keyword"] = "India politics", ["lang"] = "eng" },
                    Category = "Politics",
                    Language = "en",
                    Enabled = true
                }
            ]
        };

        var provider = new EventRegistryProvider(
            new StubHttpClientFactory(new StubHttpMessageHandler(new Dictionary<string, string> { [url] = json })),
            ConfigWithKey(EventRegistryProvider.ProviderName, "test-key"),
            NullLogger<EventRegistryProvider>.Instance);

        var results = await provider.FetchAllEndpointsAsync(options, CancellationToken.None);

        var result = Assert.Single(results);
        Assert.True(result.Success);
        var article = Assert.Single(result.Articles);
        Assert.Equal("Event Registry Headline", article.Title);
        Assert.Equal("Example Source", article.Source);
        Assert.Equal("John Author", article.Author);
        Assert.Equal("1234567890", article.OriginalGuid);
        Assert.Equal(ArticleSourceType.Api, article.SourceType);
    }

    [Fact]
    public async Task EventRegistryProvider_NoApiKeyConfigured_ReturnsUnsuccessfulResultInsteadOfThrowing()
    {
        var options = new NewsApiProviderOptions
        {
            Name = EventRegistryProvider.ProviderName,
            BaseUrl = "https://eventregistry.org/api/v1/article/getArticles",
            TimeoutSeconds = 30,
            Endpoints =
            [
                new NewsApiEndpointOptions { Name = "GetArticles", Endpoint = "", Enabled = true }
            ]
        };

        var provider = new EventRegistryProvider(
            new StubHttpClientFactory(new StubHttpMessageHandler(new Dictionary<string, string>())),
            new ConfigurationBuilder().Build(),
            NullLogger<EventRegistryProvider>.Instance);

        var results = await provider.FetchAllEndpointsAsync(options, CancellationToken.None);

        var result = Assert.Single(results);
        Assert.False(result.Success);
        Assert.Contains("NewsApiKeys:EventRegistry", result.Error);
    }
}
