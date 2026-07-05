using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Joy News (myjoyonline.com, Ghana - English-language) RSS integration - standard WordPress
/// /feed/. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Ghana"]:Providers[Name="JoyNews"]:Feeds, never hardcoded here.
/// </summary>
public sealed class JoyNewsRssProvider : BaseRssProvider
{
    public const string ProviderName = "JoyNews";
    public const string ClientName = "JoyNewsRssClient";

    public JoyNewsRssProvider(IHttpClientFactory httpClientFactory, ILogger<JoyNewsRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
