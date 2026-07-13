using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// MPinfo (mpinfo.org) RSS integration - Madhya Pradesh's own Public Relations Department feeds
/// (Hindi/English general news, CM news, cabinet decisions). Feed URLs live entirely in
/// configuration under NewsCrawler:Providers[Name="MPInfo"]:Feeds, never hardcoded here.
/// </summary>
public sealed class MPInfoRssProvider : BaseRssProvider
{
    public const string ProviderName = "MPInfo";
    public const string ClientName = "MPInfoRssClient";

    public MPInfoRssProvider(IHttpClientFactory httpClientFactory, ILogger<MPInfoRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
