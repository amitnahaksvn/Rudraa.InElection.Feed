using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Malay Mail (malaymail.com, Malaysia - English-language) RSS integration -
/// malaymail.com/feed/rss. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Malaysia"]:Providers[Name="MalayMail"]:Feeds, never hardcoded
/// here.
/// </summary>
public sealed class MalayMailRssProvider : BaseRssProvider
{
    public const string ProviderName = "MalayMail";
    public const string ClientName = "MalayMailRssClient";

    public MalayMailRssProvider(IHttpClientFactory httpClientFactory, ILogger<MalayMailRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
