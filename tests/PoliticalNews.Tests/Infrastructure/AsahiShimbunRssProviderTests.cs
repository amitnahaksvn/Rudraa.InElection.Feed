using Microsoft.Extensions.Logging.Abstractions;
using Application.Options;
using Infrastructure.RssProviders;
using PoliticalNews.Tests.TestSupport;

namespace PoliticalNews.Tests.Infrastructure;

public class AsahiShimbunRssProviderTests
{
    private const string FeedUrl = "https://example.com/rss/asahi-headlines.rdf";
    private const string ArticleUrl = "http://www.asahi.com/articles/example.html";

    /// <summary>
    /// Mirrors Asahi's real RSS 1.0/RDF quirks: &lt;item rdf:about="..."&gt; (still just an "item"
    /// element name to System.Xml.Linq, same as CBC's namespaced attribute) and no &lt;pubDate&gt;
    /// at all - only Dublin Core's &lt;dc:date&gt;.
    /// </summary>
    private const string SampleRdf = $$"""
        <rdf:RDF xmlns:rdf="http://www.w3.org/1999/02/22-rdf-syntax-ns#" xmlns:dc="http://purl.org/dc/elements/1.1/">
        <channel>
         <title>The Asahi Shimbun</title>
        </channel>
        <item rdf:about="{{ArticleUrl}}">
            <title>Test Headline One</title>
            <link>{{ArticleUrl}}</link>
            <description></description>
            <dc:subject>Category</dc:subject>
            <dc:date>2026-07-05T20:00:00+09:00</dc:date>
        </item>
        </rdf:RDF>
        """;

    [Fact]
    public async Task FetchAllFeedsAsync_NoPubDate_FallsBackToDublinCoreDate()
    {
        var handler = new StubHttpMessageHandler(new Dictionary<string, string>
        {
            [FeedUrl] = SampleRdf,
            [ArticleUrl] = "<html><head></head><body>no og:image here</body></html>"
        });
        var provider = new AsahiShimbunRssProvider(new StubHttpClientFactory(handler), NullLogger<AsahiShimbunRssProvider>.Instance);

        var feeds = new List<RssFeedOptions>
        {
            new() { Name = "Latest", Url = FeedUrl, Category = "General", Language = "ja", Enabled = true }
        };

        var results = await provider.FetchAllFeedsAsync(feeds, CancellationToken.None);

        var result = Assert.Single(results);
        Assert.True(result.Success);

        // <item rdf:about="..."> is still just an "item" element to XDocument.Descendants("item").
        var article = Assert.Single(result.Articles);
        Assert.Equal("Test Headline One", article.Title);

        // No <pubDate> in this feed at all - must fall back to <dc:date> rather than staying null.
        Assert.NotNull(article.PublishedAt);
        Assert.Equal(new DateTimeOffset(2026, 7, 5, 20, 0, 0, TimeSpan.FromHours(9)), article.PublishedAt);
    }
}
