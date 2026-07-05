using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Global New Light of Myanmar (gnlm.com.mm, Myanmar - English-language, the military
/// government's official English-language paper) RSS integration - standard WordPress /feed/.
/// Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Myanmar"]:Providers[Name="GlobalNewLightOfMyanmar"]:Feeds, never
/// hardcoded here.
/// </summary>
public sealed class GlobalNewLightOfMyanmarRssProvider : BaseRssProvider
{
    public const string ProviderName = "GlobalNewLightOfMyanmar";
    public const string ClientName = "GlobalNewLightOfMyanmarRssClient";

    public GlobalNewLightOfMyanmarRssProvider(IHttpClientFactory httpClientFactory, ILogger<GlobalNewLightOfMyanmarRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
