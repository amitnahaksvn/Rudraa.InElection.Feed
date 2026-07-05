using Microsoft.Extensions.Logging;

namespace Infrastructure.RssProviders;

/// <summary>
/// Inquirer (newsinfo.inquirer.net, Philippines - English-language) RSS integration -
/// newsinfo.inquirer.net/feed. Feed URL lives entirely in configuration under
/// NewsCrawler:Countries[Name="Philippines"]:Providers[Name="Inquirer"]:Feeds, never hardcoded
/// here.
/// </summary>
public sealed class InquirerRssProvider : BaseRssProvider
{
    public const string ProviderName = "Inquirer";
    public const string ClientName = "InquirerRssClient";

    public InquirerRssProvider(IHttpClientFactory httpClientFactory, ILogger<InquirerRssProvider> logger)
        : base(httpClientFactory, logger)
    {
    }

    public override string Name => ProviderName;

    protected override string HttpClientName => ClientName;
}
