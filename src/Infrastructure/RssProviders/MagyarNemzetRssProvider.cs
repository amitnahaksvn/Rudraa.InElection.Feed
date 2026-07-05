using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Magyar Nemzet (magyarnemzet.hu, Hungary - Hungarian-language daily) RSS integration -
/// magyarnemzet.hu/feed. The user's source table labeled this row "MTI (Hungarian News Agency)",
/// but the feed's own &lt;title&gt; and content are unambiguously Magyar Nemzet (a conservative
/// Hungarian daily), not MTI (the state news agency, a different, unrelated publisher with no
/// discoverable public feed of its own) - named for what the feed actually is rather than the
/// requested label, same "verify, don't just trust the label" discipline as every other provider
/// in this codebase. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Hungary"]:Providers[Name="MagyarNemzet"]:Feeds, never hardcoded
/// here.
/// </summary>
public sealed class MagyarNemzetRssProvider : BaseRssProvider
{
    public const string ProviderName = "MagyarNemzet";
    public const string ClientName = "MagyarNemzetRssClient";

    public MagyarNemzetRssProvider(IHttpClientFactory httpClientFactory, ILogger<MagyarNemzetRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
