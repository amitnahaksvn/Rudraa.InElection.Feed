using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Guardian Australia (theguardian.com/australia-news, Australia edition) RSS integration -
/// a separate provider from the UK TheGuardian one since it's editorially a distinct edition, not
/// just another category - theguardian.com/australia-news/rss. The requested "Politics" edition
/// feed (australia-news/politics/rss) 404s, and the requested "World" feed is the exact same URL
/// (theguardian.com/world/rss) already configured under TheGuardian in the United Kingdom block,
/// so re-adding it here would just double-fetch the identical URL every tick for no benefit -
/// both are deliberately excluded. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Australia"]:Providers[Name="GuardianAustralia"]:Feeds, never
/// hardcoded here.
/// </summary>
public sealed class GuardianAustraliaRssProvider : BaseRssProvider
{
    public const string ProviderName = "GuardianAustralia";
    public const string ClientName = "GuardianAustraliaRssClient";

    public GuardianAustraliaRssProvider(IHttpClientFactory httpClientFactory, ILogger<GuardianAustraliaRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
