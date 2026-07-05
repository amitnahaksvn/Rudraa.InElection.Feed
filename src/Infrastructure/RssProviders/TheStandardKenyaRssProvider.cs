using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// The Standard (standardmedia.co.ke, Kenya - English-language; named ...Kenya to disambiguate
/// from the existing US "TheHill"-adjacent/UK-style "Standard"-named outlets elsewhere in this
/// codebase, e.g. DerStandard/EveningStandard) RSS integration - the requested /rss 404s, but
/// resolves to an index page listing real per-section feeds at /rss/{section}.php; three are
/// wired up (Headlines/Kenya/Politics). Feed URLs live entirely in configuration under
/// NewsCrawler:Countries[Name="Kenya"]:Providers[Name="TheStandardKenya"]:Feeds, never hardcoded
/// here.
/// </summary>
public sealed class TheStandardKenyaRssProvider : BaseRssProvider
{
    public const string ProviderName = "TheStandardKenya";
    public const string ClientName = "TheStandardKenyaRssClient";

    public TheStandardKenyaRssProvider(IHttpClientFactory httpClientFactory, ILogger<TheStandardKenyaRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
