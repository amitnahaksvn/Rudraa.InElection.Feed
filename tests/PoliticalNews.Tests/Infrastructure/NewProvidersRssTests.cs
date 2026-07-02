using Microsoft.Extensions.Logging.Abstractions;
using PoliticalNews.Application.Abstractions;
using PoliticalNews.Application.Options;
using PoliticalNews.Infrastructure.RssProviders;
using PoliticalNews.Tests.TestSupport;

namespace PoliticalNews.Tests.Infrastructure;

/// <summary>
/// IndiaTV/News18/NDTV/IndianExpress/TheHindu all share BaseRssProvider's pipeline unchanged
/// (verified against each site's real XML: spec-cased pubDate, media:thumbnail/media:content
/// image tags), so one parameterized parse test per provider covers the provider-specific part:
/// the Name stamped onto every normalized article.
/// </summary>
public class NewProvidersRssTests
{
    private const string FeedUrl = "https://example.com/feed.xml";

    private const string SampleRss = """
        <?xml version="1.0" encoding="UTF-8"?>
        <rss version="2.0" xmlns:media="http://search.yahoo.com/mrss/">
          <channel>
            <title>Sample</title>
            <item>
              <title>Test Headline One</title>
              <link><![CDATA[https://example.com/story-1]]></link>
              <guid isPermaLink="false">guid-1</guid>
              <pubDate><![CDATA[Thu, 02 Jul 2026 16:20:50 +0530]]></pubDate>
              <description>Summary of story one</description>
              <media:content url="https://example.com/image1.jpg" medium="image" />
            </item>
          </channel>
        </rss>
        """;

    public static TheoryData<string, Func<IHttpClientFactory, IRssProvider>> Providers => new()
    {
        { "IndiaTV", f => new IndiaTvRssProvider(f, NullLogger<IndiaTvRssProvider>.Instance) },
        { "News18", f => new News18RssProvider(f, NullLogger<News18RssProvider>.Instance) },
        { "NDTV", f => new NdtvRssProvider(f, NullLogger<NdtvRssProvider>.Instance) },
        { "IndianExpress", f => new IndianExpressRssProvider(f, NullLogger<IndianExpressRssProvider>.Instance) },
        { "TheHindu", f => new TheHinduRssProvider(f, NullLogger<TheHinduRssProvider>.Instance) }
    };

    [Theory]
    [MemberData(nameof(Providers))]
    public async Task FetchAllFeedsAsync_StampsProviderNameAndParsesItem(
        string expectedName, Func<IHttpClientFactory, IRssProvider> create)
    {
        var handler = new StubHttpMessageHandler(new Dictionary<string, string> { [FeedUrl] = SampleRss });
        var provider = create(new StubHttpClientFactory(handler));

        Assert.Equal(expectedName, provider.Name);

        var feeds = new List<RssFeedOptions>
        {
            new() { Name = "Home", Url = FeedUrl, Category = "General", Language = "en", Enabled = true }
        };

        var results = await provider.FetchAllFeedsAsync(feeds, CancellationToken.None);

        var article = Assert.Single(Assert.Single(results).Articles);
        Assert.Equal(expectedName, article.Provider);
        Assert.Equal("Test Headline One", article.Title);
        Assert.Equal("https://example.com/story-1", article.Url);
        Assert.Equal("https://example.com/image1.jpg", article.ImageUrl);
        Assert.NotNull(article.PublishedAt);
    }
}
