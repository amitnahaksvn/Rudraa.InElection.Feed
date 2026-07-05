using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Proto Thema (protothema.gr, Greece - Greek-language) RSS integration - protothema.gr/rss.
/// Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Greece"]:Providers[Name="ProtoThema"]:Feeds, never hardcoded here.
/// </summary>
public sealed class ProtoThemaRssProvider : BaseRssProvider
{
    public const string ProviderName = "ProtoThema";
    public const string ClientName = "ProtoThemaRssClient";

    public ProtoThemaRssProvider(IHttpClientFactory httpClientFactory, ILogger<ProtoThemaRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
