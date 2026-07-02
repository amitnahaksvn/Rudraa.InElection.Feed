using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Press Information Bureau (pib.gov.in) RSS integration - feeds live under
/// RssMain.aspx?ModId={category}&amp;lang={language}, discovered by probing the ASP.NET
/// WebForms query-parameter scheme directly (ModId=6 press releases, ModId=8 photo features,
/// ModId=9 media invitations; lang=1 English, lang=2 Hindi, lang=3 Urdu - lang values above 3 all
/// silently fall back to Hindi content, so there is no genuine "regional" language variant beyond
/// these three). Feed URLs live entirely in configuration under
/// NewsCrawler:Providers[Name="PIB"]:Feeds, never hardcoded here.
/// </summary>
public sealed class PibRssProvider : BaseRssProvider
{
    public const string ProviderName = "PIB";
    public const string ClientName = "PibRssClient";

    public PibRssProvider(IHttpClientFactory httpClientFactory, ILogger<PibRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
