using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// RTP Noticias (rtp.pt, Portugal - Portuguese-language public broadcaster) RSS integration -
/// rtp.pt/noticias/rss. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Portugal"]:Providers[Name="RTPNoticias"]:Feeds, never hardcoded
/// here.
/// </summary>
public sealed class RtpNoticiasRssProvider : BaseRssProvider
{
    public const string ProviderName = "RTPNoticias";
    public const string ClientName = "RtpNoticiasRssClient";

    public RtpNoticiasRssProvider(IHttpClientFactory httpClientFactory, ILogger<RtpNoticiasRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
