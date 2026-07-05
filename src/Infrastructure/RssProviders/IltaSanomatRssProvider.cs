using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Ilta-Sanomat (is.fi, Finland - Finnish-language) RSS integration - is.fi/rss/tuoreimmat.xml.
/// Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Finland"]:Providers[Name="IltaSanomat"]:Feeds, never hardcoded
/// here.
/// </summary>
public sealed class IltaSanomatRssProvider : BaseRssProvider
{
    public const string ProviderName = "IltaSanomat";
    public const string ClientName = "IltaSanomatRssClient";

    public IltaSanomatRssProvider(IHttpClientFactory httpClientFactory, ILogger<IltaSanomatRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
