using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Radio Prague International (english.radio.cz, Czech Republic - English-language edition) RSS
/// integration - the requested english.radio.cz/rss 404s; the real feed, declared via a
/// rel="alternate" link tag on the homepage, is english.radio.cz/rcz-rss/en. Feed URL lives
/// entirely in configuration under
/// NewsCrawler:Countries[Name="Czech Republic"]:Providers[Name="RadioPragueInternational"]:Feeds,
/// never hardcoded here.
/// </summary>
public sealed class RadioPragueInternationalRssProvider : BaseRssProvider
{
    public const string ProviderName = "RadioPragueInternational";
    public const string ClientName = "RadioPragueInternationalRssClient";

    public RadioPragueInternationalRssProvider(IHttpClientFactory httpClientFactory, ILogger<RadioPragueInternationalRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
