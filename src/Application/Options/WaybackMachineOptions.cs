namespace Application.Options;

/// <summary>
/// Free Internet Archive S3-style API credentials (from archive.org/account/s3.php - a free
/// account, no payment) used by <see cref="Infrastructure.RssProviders.WaybackMachineFeedResolver"/>
/// to route MPInfo/NDMA fetches through the Wayback Machine's "Save Page Now" API instead of
/// fetching those two providers directly. Both are confirmed network-blocked against every cloud
/// IP range tested (Azure App Service, Cloudflare Workers, GitHub Actions runners), while the
/// Wayback Machine's own crawler is confirmed to reach both successfully - see
/// <see cref="Infrastructure.RssProviders.WaybackMachineFeedResolver"/>'s own doc comment for the
/// full mechanism. Both fields optional - falls back to a direct (unproxied) fetch when unset, same
/// as every other environment-optional option in this codebase.
/// </summary>
public sealed class WaybackMachineOptions
{
    public const string SectionName = "WaybackMachine";
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
}
