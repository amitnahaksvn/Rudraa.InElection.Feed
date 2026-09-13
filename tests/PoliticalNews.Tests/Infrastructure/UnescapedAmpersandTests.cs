using Microsoft.Extensions.Logging.Abstractions;
using Application.Options;
using Infrastructure.RssProviders;
using PoliticalNews.Tests.TestSupport;

namespace PoliticalNews.Tests.Infrastructure;

/// <summary>
/// National Herald's feed generator emits raw, un-escaped "&amp;" characters (an HTML entity like
/// "&amp;nbsp;", or plain text like "Politics &amp; Economy") that XML requires written as
/// "&amp;amp;" - confirmed live via a recurring production XmlException
/// ("An error occurred while parsing EntityName"). Covers both pipelines that parse raw feed XML
/// (BaseRssProvider's RSS 2.0 and BaseAtomRssProvider's Atom 1.0), since the fix was applied to
/// each separately rather than through one shared base class.
/// </summary>
public class UnescapedAmpersandTests
{
    private const string FeedUrl = "https://example.com/feed.xml";

    private const string SampleRssWithUnescapedAmpersand = $$"""
        <?xml version="1.0" encoding="UTF-8"?>
        <rss version="2.0">
          <channel>
            <title>Sample</title>
            <item>
              <title>Test Headline One</title>
              <link>https://example.com/story-1</link>
              <guid isPermaLink="false">guid-1</guid>
              <pubDate>Thu, 02 Jul 2026 16:20:50 +0530</pubDate>
              <description>Politics & Economy</description>
            </item>
          </channel>
        </rss>
        """;

    private const string SampleAtomWithUnescapedAmpersand = $$"""
        <feed xmlns="http://www.w3.org/2005/Atom">
          <title>Sample</title>
          <entry>
            <title>Test Headline One</title>
            <id>https://example.com/story-1</id>
            <link href="https://example.com/story-1" />
            <published>2026-07-05T20:00:00+05:30</published>
            <summary>Politics & Economy</summary>
          </entry>
        </feed>
        """;

    [Fact]
    public async Task BaseRssProvider_FetchAllFeedsAsync_ToleratesUnescapedAmpersand()
    {
        var handler = new StubHttpMessageHandler(new Dictionary<string, string> { [FeedUrl] = SampleRssWithUnescapedAmpersand });
        var provider = new AajTakRssProvider(new StubHttpClientFactory(handler), NullLogger<AajTakRssProvider>.Instance);

        var feeds = new List<RssFeedOptions>
        {
            new() { Name = "Home", Url = FeedUrl, Category = "General", Language = "en", Enabled = true }
        };

        var results = await provider.FetchAllFeedsAsync(feeds, CancellationToken.None);

        var result = Assert.Single(results);
        Assert.True(result.Success);
        var article = Assert.Single(result.Articles);
        Assert.Equal("Test Headline One", article.Title);
    }

    [Fact]
    public async Task BaseAtomRssProvider_FetchAllFeedsAsync_ToleratesUnescapedAmpersand()
    {
        var handler = new StubHttpMessageHandler(new Dictionary<string, string> { [FeedUrl] = SampleAtomWithUnescapedAmpersand });
        var provider = new NationalHeraldRssProvider(new StubHttpClientFactory(handler), NullLogger<NationalHeraldRssProvider>.Instance);

        var feeds = new List<RssFeedOptions>
        {
            new() { Name = "Home", Url = FeedUrl, Category = "General", Language = "en", Enabled = true }
        };

        var results = await provider.FetchAllFeedsAsync(feeds, CancellationToken.None);

        var result = Assert.Single(results);
        Assert.True(result.Success);
        var article = Assert.Single(result.Articles);
        Assert.Equal("Test Headline One", article.Title);
    }
}
