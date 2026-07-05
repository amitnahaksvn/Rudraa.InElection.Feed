using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// CNBC (cnbc.com, United States) RSS integration - the classic
/// cnbc.com/id/{numericId}/device/rss/rss.html pattern (a legacy CMS-id scheme, not a
/// human-readable slug). Feed URLs live entirely in configuration under
/// NewsCrawler:Countries[Name="United States"]:Providers[Name="CNBC"]:Feeds, never hardcoded here.
/// </summary>
public sealed class CnbcRssProvider : BaseRssProvider
{
    public const string ProviderName = "CNBC";
    public const string ClientName = "CnbcRssClient";

    public CnbcRssProvider(IHttpClientFactory httpClientFactory, ILogger<CnbcRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
