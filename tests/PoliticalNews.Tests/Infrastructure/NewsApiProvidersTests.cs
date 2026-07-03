using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Application.Options;
using Infrastructure.NewsApiProviders;
using PoliticalNews.Tests.TestSupport;

namespace PoliticalNews.Tests.Infrastructure;

/// <summary>
/// One happy-path parse test per JSON news-API provider - the only realistic way to verify each
/// provider's response-shape assumptions without a live API key (none were available while these
/// were built), plus the shared "missing API key" and "HTTP failure" behaviours every provider
/// inherits from <see cref="BaseNewsApiProvider"/>, and a check that a provider with two enabled
/// endpoints fetches both.
/// </summary>
public class NewsApiProvidersTests
{
    private static IConfiguration ConfigWithKey(string providerName, string key = "test-key") =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { [$"NewsApiKeys:{providerName}"] = key })
            .Build();

    [Fact]
    public async Task NewsApiOrgProvider_ParsesArticles_AcrossMultipleEndpoints()
    {
        const string everythingUrl = "https://newsapi.org/v2/everything?q=India&apiKey=test-key";
        const string everythingJson = """
            {
              "status": "ok",
              "totalResults": 1,
              "articles": [
                {
                  "source": { "id": null, "name": "Example News" },
                  "author": "Jane Doe",
                  "title": "Test Headline One",
                  "description": "Summary one",
                  "url": "https://example.com/story-1",
                  "urlToImage": "https://example.com/image1.jpg",
                  "publishedAt": "2026-07-01T10:00:00Z",
                  "content": "Full content one"
                }
              ]
            }
            """;

        const string topHeadlinesUrl = "https://newsapi.org/v2/top-headlines?country=in&apiKey=test-key";
        const string topHeadlinesJson = """
            {
              "status": "ok",
              "totalResults": 1,
              "articles": [
                {
                  "source": { "id": null, "name": "Example Headlines" },
                  "author": "John Doe",
                  "title": "Top Headline One",
                  "description": "Top summary",
                  "url": "https://example.com/top-1",
                  "urlToImage": "https://example.com/top1.jpg",
                  "publishedAt": "2026-07-01T11:00:00Z",
                  "content": "Top content"
                }
              ]
            }
            """;

        var options = new NewsApiProviderOptions
        {
            Name = NewsApiOrgProvider.ProviderName,
            BaseUrl = "https://newsapi.org/v2",
            AuthType = ApiAuthType.QueryParameter,
            AuthParamName = "apiKey",
            TimeoutSeconds = 30,
            Endpoints =
            [
                new NewsApiEndpointOptions
                {
                    Name = "Everything",
                    Endpoint = "everything",
                    QueryParameters = new Dictionary<string, string> { ["q"] = "India" },
                    Category = "Politics",
                    Language = "en",
                    Enabled = true
                },
                new NewsApiEndpointOptions
                {
                    Name = "TopHeadlines",
                    Endpoint = "top-headlines",
                    QueryParameters = new Dictionary<string, string> { ["country"] = "in" },
                    Category = "General",
                    Language = "en",
                    Enabled = true
                }
            ]
        };

        var provider = new NewsApiOrgProvider(
            new StubHttpClientFactory(new StubHttpMessageHandler(new Dictionary<string, string>
            {
                [everythingUrl] = everythingJson,
                [topHeadlinesUrl] = topHeadlinesJson
            })),
            ConfigWithKey(NewsApiOrgProvider.ProviderName),
            NullLogger<NewsApiOrgProvider>.Instance);

        var results = await provider.FetchAllEndpointsAsync(options, CancellationToken.None);

        Assert.Equal(2, results.Count);
        Assert.All(results, r => Assert.True(r.Success));

        var everythingResult = Assert.Single(results, r => r.EndpointName == "Everything");
        var everythingArticle = Assert.Single(everythingResult.Articles);
        Assert.Equal("Test Headline One", everythingArticle.Title);
        Assert.Equal("Politics", everythingArticle.Category);

        var topHeadlinesResult = Assert.Single(results, r => r.EndpointName == "TopHeadlines");
        var topHeadlinesArticle = Assert.Single(topHeadlinesResult.Articles);
        Assert.Equal("Top Headline One", topHeadlinesArticle.Title);
        Assert.Equal("General", topHeadlinesArticle.Category);
    }

    [Fact]
    public async Task GNewsProvider_ParsesArticles()
    {
        const string url = "https://gnews.io/api/v4/search?q=India&apikey=test-key";
        const string json = """
            {
              "totalArticles": 1,
              "articles": [
                {
                  "title": "GNews Headline",
                  "description": "GNews summary",
                  "content": "GNews content",
                  "url": "https://example.com/gnews-1",
                  "image": "https://example.com/gnews.jpg",
                  "publishedAt": "2026-07-01T09:00:00Z",
                  "lang": "en",
                  "source": { "name": "GNews Source", "url": "https://source.example.com" }
                }
              ]
            }
            """;

        var options = SingleEndpointOptions(
            GNewsProvider.ProviderName, "https://gnews.io/api/v4", "apikey",
            "Search", "search", ("q", "India"));

        var provider = new GNewsProvider(
            new StubHttpClientFactory(new StubHttpMessageHandler(new Dictionary<string, string> { [url] = json })),
            ConfigWithKey(GNewsProvider.ProviderName),
            NullLogger<GNewsProvider>.Instance);

        var results = await provider.FetchAllEndpointsAsync(options, CancellationToken.None);

        var result = Assert.Single(results);
        Assert.True(result.Success);
        var article = Assert.Single(result.Articles);
        Assert.Equal("GNews Headline", article.Title);
        Assert.Equal("GNews Source", article.Source);
        Assert.Equal("https://example.com/gnews.jpg", article.ImageUrl);
    }

    [Fact]
    public async Task TheNewsApiProvider_ParsesArticles()
    {
        const string url = "https://api.thenewsapi.com/v1/news/all?search=India&api_token=test-key";
        const string json = """
            {
              "meta": { "found": 1, "returned": 1, "limit": 25, "page": 1 },
              "data": [
                {
                  "uuid": "abc-123",
                  "title": "TheNewsAPI Headline",
                  "description": "TheNewsAPI summary",
                  "keywords": "india,politics",
                  "snippet": "First part of body",
                  "url": "https://example.com/thenewsapi-1",
                  "image_url": "https://example.com/thenewsapi.jpg",
                  "language": "en",
                  "published_at": "2026-07-01T08:00:00.000000Z",
                  "source": "example.com",
                  "categories": ["politics", "general"]
                }
              ]
            }
            """;

        var options = SingleEndpointOptions(
            TheNewsApiProvider.ProviderName, "https://api.thenewsapi.com/v1/news", "api_token",
            "All", "all", ("search", "India"));

        var provider = new TheNewsApiProvider(
            new StubHttpClientFactory(new StubHttpMessageHandler(new Dictionary<string, string> { [url] = json })),
            ConfigWithKey(TheNewsApiProvider.ProviderName),
            NullLogger<TheNewsApiProvider>.Instance);

        var results = await provider.FetchAllEndpointsAsync(options, CancellationToken.None);

        var result = Assert.Single(results);
        Assert.True(result.Success);
        var article = Assert.Single(result.Articles);
        Assert.Equal("TheNewsAPI Headline", article.Title);
        Assert.Equal("abc-123", article.OriginalGuid);
        Assert.Equal("politics", article.Category);
        Assert.Contains("politics", article.Tags);
    }

    [Fact]
    public async Task CurrentsApiProvider_ParsesArticles_AndTreatsLiteralNoneImageAsMissing()
    {
        const string url = "https://api.currentsapi.services/v1/latest-news?language=en&apiKey=test-key";
        const string json = """
            {
              "status": "ok",
              "news": [
                {
                  "id": "cur-1",
                  "title": "Currents Headline",
                  "description": "Currents summary",
                  "url": "https://example.com/currents-1",
                  "author": "Currents Author",
                  "image": "None",
                  "language": "en",
                  "category": ["politics"],
                  "published": "2026-07-01T07:00:00 +0000"
                }
              ]
            }
            """;

        var options = SingleEndpointOptions(
            CurrentsApiProvider.ProviderName, "https://api.currentsapi.services/v1", "apiKey",
            "LatestNews", "latest-news", ("language", "en"));

        var provider = new CurrentsApiProvider(
            new StubHttpClientFactory(new StubHttpMessageHandler(new Dictionary<string, string> { [url] = json })),
            ConfigWithKey(CurrentsApiProvider.ProviderName),
            NullLogger<CurrentsApiProvider>.Instance);

        var results = await provider.FetchAllEndpointsAsync(options, CancellationToken.None);

        var result = Assert.Single(results);
        Assert.True(result.Success);
        var article = Assert.Single(result.Articles);
        Assert.Equal("Currents Headline", article.Title);
        Assert.Null(article.ImageUrl);
        Assert.Equal("politics", article.Category);
    }

    [Fact]
    public async Task MediastackProvider_ParsesArticles()
    {
        const string url = "http://api.mediastack.com/v1/news?keywords=India&access_key=test-key";
        const string json = """
            {
              "pagination": { "limit": 25, "offset": 0, "count": 1, "total": 1 },
              "data": [
                {
                  "author": "Mediastack Author",
                  "title": "Mediastack Headline",
                  "description": "Mediastack summary",
                  "url": "https://example.com/mediastack-1",
                  "source": "Example Source",
                  "image": "https://example.com/mediastack.jpg",
                  "category": "general",
                  "language": "en",
                  "country": "in",
                  "published_at": "2026-07-01T06:00:00+00:00"
                }
              ]
            }
            """;

        var options = SingleEndpointOptions(
            MediastackProvider.ProviderName, "http://api.mediastack.com/v1", "access_key",
            "News", "news", ("keywords", "India"));

        var provider = new MediastackProvider(
            new StubHttpClientFactory(new StubHttpMessageHandler(new Dictionary<string, string> { [url] = json })),
            ConfigWithKey(MediastackProvider.ProviderName),
            NullLogger<MediastackProvider>.Instance);

        var results = await provider.FetchAllEndpointsAsync(options, CancellationToken.None);

        var result = Assert.Single(results);
        Assert.True(result.Success);
        var article = Assert.Single(result.Articles);
        Assert.Equal("Mediastack Headline", article.Title);
        Assert.Equal("Example Source", article.Source);
        Assert.Equal("general", article.Category);
    }

    [Fact]
    public async Task NewsDataIoProvider_ParsesArticles()
    {
        const string url = "https://newsdata.io/api/1/latest?q=India&apikey=test-key";
        const string json = """
            {
              "status": "success",
              "totalResults": 1,
              "results": [
                {
                  "article_id": "nd-1",
                  "title": "NewsData Headline",
                  "link": "https://example.com/newsdata-1",
                  "description": "NewsData summary",
                  "content": "NewsData content",
                  "pubDate": "2026-07-01 05:00:00",
                  "image_url": "https://example.com/newsdata.jpg",
                  "source_id": "example",
                  "source_url": "https://example.com",
                  "language": "en",
                  "country": ["india"],
                  "category": ["politics"],
                  "creator": ["NewsData Author"]
                }
              ],
              "nextPage": null
            }
            """;

        var options = SingleEndpointOptions(
            NewsDataIoProvider.ProviderName, "https://newsdata.io/api/1", "apikey",
            "Latest", "latest", ("q", "India"));

        var provider = new NewsDataIoProvider(
            new StubHttpClientFactory(new StubHttpMessageHandler(new Dictionary<string, string> { [url] = json })),
            ConfigWithKey(NewsDataIoProvider.ProviderName),
            NullLogger<NewsDataIoProvider>.Instance);

        var results = await provider.FetchAllEndpointsAsync(options, CancellationToken.None);

        var result = Assert.Single(results);
        Assert.True(result.Success);
        var article = Assert.Single(result.Articles);
        Assert.Equal("NewsData Headline", article.Title);
        Assert.Equal("NewsData Author", article.Author);
        Assert.Equal("nd-1", article.OriginalGuid);
        Assert.Equal("politics", article.Category);
    }

    [Fact]
    public async Task WorldNewsApiProvider_ParsesArticles_UsesHeaderAuthNotQueryParam()
    {
        // No apiKey/x-api-key in the URL itself - proves HttpHeader auth doesn't leak into the query string.
        const string url = "https://api.worldnewsapi.com/search-news?text=India";
        const string json = """
            {
              "available": 1,
              "news": [
                {
                  "id": 42,
                  "title": "World News Headline",
                  "text": "World News body",
                  "summary": "World News summary",
                  "url": "https://example.com/worldnews-1",
                  "image": "https://example.com/worldnews.jpg",
                  "publish_date": "2026-07-01 04:00:00",
                  "authors": ["World News Author"],
                  "language": "en",
                  "category": "politics",
                  "source_country": "in"
                }
              ]
            }
            """;

        var options = SingleEndpointOptions(
            WorldNewsApiProvider.ProviderName, "https://api.worldnewsapi.com", "x-api-key",
            "SearchNews", "search-news", ("text", "India"), ApiAuthType.HttpHeader);

        var provider = new WorldNewsApiProvider(
            new StubHttpClientFactory(new StubHttpMessageHandler(new Dictionary<string, string> { [url] = json })),
            ConfigWithKey(WorldNewsApiProvider.ProviderName),
            NullLogger<WorldNewsApiProvider>.Instance);

        var results = await provider.FetchAllEndpointsAsync(options, CancellationToken.None);

        var result = Assert.Single(results);
        Assert.True(result.Success);
        var article = Assert.Single(result.Articles);
        Assert.Equal("World News Headline", article.Title);
        Assert.Equal("42", article.OriginalGuid);
        Assert.Equal("World News Author", article.Author);
    }

    [Fact]
    public async Task FetchAllEndpointsAsync_NoApiKeyConfigured_ReturnsUnsuccessfulResultInsteadOfThrowing()
    {
        var options = SingleEndpointOptions(
            NewsApiOrgProvider.ProviderName, "https://newsapi.org/v2", "apiKey",
            "Everything", "everything", ("q", "India"));

        var provider = new NewsApiOrgProvider(
            new StubHttpClientFactory(new StubHttpMessageHandler(new Dictionary<string, string>())),
            new ConfigurationBuilder().Build(),
            NullLogger<NewsApiOrgProvider>.Instance);

        var results = await provider.FetchAllEndpointsAsync(options, CancellationToken.None);

        var result = Assert.Single(results);
        Assert.False(result.Success);
        Assert.Contains("NewsApiKeys:NewsApiOrg", result.Error);
    }

    [Fact]
    public async Task FetchAllEndpointsAsync_HttpFailure_ReturnsUnsuccessfulResultInsteadOfThrowing()
    {
        var options = SingleEndpointOptions(
            NewsApiOrgProvider.ProviderName, "https://newsapi.org/v2", "apiKey",
            "Everything", "everything", ("q", "India"));

        var provider = new NewsApiOrgProvider(
            new StubHttpClientFactory(new StubHttpMessageHandler(new Dictionary<string, string>())),
            ConfigWithKey(NewsApiOrgProvider.ProviderName),
            NullLogger<NewsApiOrgProvider>.Instance);

        var results = await provider.FetchAllEndpointsAsync(options, CancellationToken.None);

        var result = Assert.Single(results);
        Assert.False(result.Success);
        Assert.NotNull(result.Error);
    }

    private static NewsApiProviderOptions SingleEndpointOptions(
        string providerName,
        string baseUrl,
        string authParamName,
        string endpointName,
        string endpointPath,
        (string Key, string Value) queryParameter,
        ApiAuthType authType = ApiAuthType.QueryParameter) =>
        new()
        {
            Name = providerName,
            BaseUrl = baseUrl,
            AuthType = authType,
            AuthParamName = authParamName,
            TimeoutSeconds = 30,
            Endpoints =
            [
                new NewsApiEndpointOptions
                {
                    Name = endpointName,
                    Endpoint = endpointPath,
                    QueryParameters = new Dictionary<string, string> { [queryParameter.Key] = queryParameter.Value },
                    Category = "Politics",
                    Language = "en",
                    Enabled = true
                }
            ]
        };
}
