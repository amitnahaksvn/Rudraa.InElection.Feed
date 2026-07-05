using Microsoft.Extensions.Logging.Abstractions;
using Application.Options;
using Infrastructure.RssProviders;
using PoliticalNews.Tests.TestSupport;

namespace PoliticalNews.Tests.Infrastructure;

public class CbcNewsRssProviderTests
{
    private const string FeedUrl = "https://example.com/rss/cbc-topstories.xml";
    private const string ArticleUrl = "https://example.com/news/story-1";

    /// <summary>
    /// Mirrors CBC's real quirks: item elements carry namespaced attributes
    /// (&lt;item cbc:type="contentpackage"...&gt;, still just an "item" element name to
    /// System.Xml.Linq) and pubDate uses the "EDT" zone abbreviation .NET's parser doesn't
    /// recognize natively.
    /// </summary>
    private const string SampleRss = $$"""
        <rss xmlns:cbc="https://www.cbc.ca/rss/cbc" version="2.0">
        <channel>
         <title>CBC | Top Stories News</title>
            <item cbc:type="contentpackage" cbc:deptid="2.633" cbc:syndicate="true">
                <title>Test Headline One</title>
                <link>{{ArticleUrl}}</link>
                <description>Summary of story one</description>
                <pubDate>Wed, 24 Jun 2026 21:33:43 EDT</pubDate>
                <guid isPermaLink="false">9.7248209</guid>
            </item>
        </channel>
        </rss>
        """;

    [Fact]
    public async Task FetchAllFeedsAsync_ParsesCbcQuirks_NamespacedItemAttributeAndEdtTimezone()
    {
        var handler = new StubHttpMessageHandler(new Dictionary<string, string>
        {
            [FeedUrl] = SampleRss,
            [ArticleUrl] = "<html><head></head><body>no og:image here</body></html>"
        });
        var provider = new CbcNewsRssProvider(new StubHttpClientFactory(handler), NullLogger<CbcNewsRssProvider>.Instance);

        var feeds = new List<RssFeedOptions>
        {
            new() { Name = "TopStories", Url = FeedUrl, Category = "General", Language = "en", Enabled = true }
        };

        var results = await provider.FetchAllFeedsAsync(feeds, CancellationToken.None);

        var result = Assert.Single(results);
        Assert.True(result.Success);

        // <item cbc:type="..."> is still just an "item" element to XDocument.Descendants("item").
        var article = Assert.Single(result.Articles);
        Assert.Equal("Test Headline One", article.Title);

        // "EDT" must resolve to the fixed -04:00 offset, not fail to parse.
        Assert.NotNull(article.PublishedAt);
        Assert.Equal(new DateTimeOffset(2026, 6, 24, 21, 33, 43, TimeSpan.FromHours(-4)), article.PublishedAt);
    }
}
