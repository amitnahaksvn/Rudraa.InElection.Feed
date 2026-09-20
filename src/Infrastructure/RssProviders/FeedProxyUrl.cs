using Application.Options;

namespace Infrastructure.RssProviders;

/// <summary>Builds the relay URL for a feed (see <see cref="FeedProxyOptions"/>) - <c>null</c> when no relay is configured.</summary>
internal static class FeedProxyUrl
{
    public static string? Build(FeedProxyOptions options, string feedUrl)
    {
        if (string.IsNullOrWhiteSpace(options.BaseUrl))
        {
            return null;
        }

        var separator = options.BaseUrl.Contains('?') ? '&' : '?';
        return $"{options.BaseUrl}{separator}url={Uri.EscapeDataString(feedUrl)}";
    }
}
