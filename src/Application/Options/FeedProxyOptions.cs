namespace Application.Options;

/// <summary>
/// Optional relay for providers whose CDN blocks this app's cloud egress IP but not other
/// networks (currently IndianExpress - CloudFront 403s Azure and GitHub Actions runners alike
/// regardless of User-Agent/headers, while residential IPs are served normally). <see cref="BaseUrl"/>
/// is a small self-hosted relay (see tools/feed-proxy-worker.js) that takes the real feed URL as a
/// <c>url</c> query parameter and only forwards hosts on its own allowlist. Unset means no relay
/// tier at all - providers fall straight through to their next fallback (Wayback).
/// </summary>
public sealed class FeedProxyOptions
{
    public const string SectionName = "FeedProxy";
    public string BaseUrl { get; set; } = string.Empty;
}
