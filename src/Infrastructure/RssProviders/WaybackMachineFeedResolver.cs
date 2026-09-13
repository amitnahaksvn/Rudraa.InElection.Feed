using System.Text.Json;
using Microsoft.Extensions.Logging;
using Application.Options;

namespace Infrastructure.RssProviders;

/// <summary>
/// Routes a feed fetch through the Internet Archive's Wayback Machine instead of fetching the
/// real URL directly - used by <see cref="MPInfoRssProvider"/>/<see cref="NdmaRssProvider"/>,
/// whose own servers block every cloud IP range this app has tested (Azure App Service,
/// Cloudflare Workers, GitHub Actions runners all confirmed network-blocked/timed out) while the
/// Wayback Machine's own crawler is confirmed able to reach both (verified live via
/// web.archive.org's public CDX index: MPInfo's exact feed URL has real HTTP-200 captures as
/// recent as 2026-02-14, proving archive.org's crawler isn't on whatever datacenter-IP blocklist
/// the other three are on).
///
/// Two-tier resolution, both talking only to archive.org (never to the blocked target directly):
/// 1. If <see cref="WaybackMachineOptions.AccessKey"/>/<see cref="WaybackMachineOptions.SecretKey"/>
///    are configured (free from archive.org/account/s3.php - a free Internet Archive account, no
///    payment), triggers a fresh on-demand capture via the authenticated "Save Page Now" (SPN2)
///    API and polls its job status for up to ~30s. This is what actually gets near-real-time data
///    (as fresh as this app's own crawl schedule), not just whatever archive.org happened to crawl
///    on its own independent schedule.
/// 2. Falls back to the public, unauthenticated CDX index for the single most recent already-
///    archived snapshot (no credentials needed) if step 1 isn't configured, fails, or times out.
///    This can be stale (archive.org's organic crawl cadence, observed roughly monthly for MPInfo)
///    or - for a URL archive.org has genuinely never crawled before (confirmed true for NDMA's
///    exact rss.xml at the time this was written) - simply unavailable.
/// If both tiers come up empty, falls back to the real URL directly (identical to today's
/// behavior - no worse than before this existed).
///
/// Never throws: every failure mode (missing credentials, archive.org down, job never completes,
/// malformed JSON) is caught internally and degrades to the next tier, since a fetch-URL resolver
/// must not be able to crash the crawl loop that calls it.
/// </summary>
internal static class WaybackMachineFeedResolver
{
    private const string SavePageNowEndpoint = "https://web.archive.org/save/";
    private const string SaveStatusEndpoint = "https://web.archive.org/save/status/";
    private const string CdxEndpoint = "https://web.archive.org/cdx/search/cdx";
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(5);
    private const int MaxPollAttempts = 6;

    public static async Task<string> ResolveAsync(
        HttpClient archiveClient,
        WaybackMachineOptions options,
        string realUrl,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(options.AccessKey) && !string.IsNullOrWhiteSpace(options.SecretKey))
        {
            var freshSnapshotUrl = await TryCaptureFreshSnapshotAsync(archiveClient, options, realUrl, logger, cancellationToken);
            if (freshSnapshotUrl is not null)
            {
                return freshSnapshotUrl;
            }
        }

        var latestSnapshotUrl = await TryGetLatestSnapshotAsync(archiveClient, realUrl, logger, cancellationToken);
        return latestSnapshotUrl ?? realUrl;
    }

    private static async Task<string?> TryCaptureFreshSnapshotAsync(
        HttpClient archiveClient,
        WaybackMachineOptions options,
        string realUrl,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        try
        {
            using var saveRequest = new HttpRequestMessage(HttpMethod.Post, SavePageNowEndpoint)
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["url"] = realUrl,
                    ["skip_first_archive"] = "1"
                })
            };
            saveRequest.Headers.Add("Accept", "application/json");
            saveRequest.Headers.Add("Authorization", $"LOW {options.AccessKey}:{options.SecretKey}");

            using var saveResponse = await archiveClient.SendAsync(saveRequest, cancellationToken);
            var saveBody = await saveResponse.Content.ReadAsStringAsync(cancellationToken);
            if (!saveResponse.IsSuccessStatusCode)
            {
                logger.LogWarning("Wayback Machine save-page request failed for {Url}: HTTP {Status} - {Body}", realUrl, (int)saveResponse.StatusCode, saveBody);
                return null;
            }

            using var saveDoc = JsonDocument.Parse(saveBody);
            if (!saveDoc.RootElement.TryGetProperty("job_id", out var jobIdElement))
            {
                logger.LogWarning("Wayback Machine save-page response for {Url} had no job_id: {Body}", realUrl, saveBody);
                return null;
            }

            var jobId = jobIdElement.GetString();
            if (string.IsNullOrWhiteSpace(jobId))
            {
                return null;
            }

            for (var attempt = 0; attempt < MaxPollAttempts; attempt++)
            {
                await Task.Delay(PollInterval, cancellationToken);

                using var statusRequest = new HttpRequestMessage(HttpMethod.Get, $"{SaveStatusEndpoint}{jobId}");
                statusRequest.Headers.Add("Accept", "application/json");
                statusRequest.Headers.Add("Authorization", $"LOW {options.AccessKey}:{options.SecretKey}");

                using var statusResponse = await archiveClient.SendAsync(statusRequest, cancellationToken);
                var statusBody = await statusResponse.Content.ReadAsStringAsync(cancellationToken);
                if (!statusResponse.IsSuccessStatusCode)
                {
                    continue;
                }

                using var statusDoc = JsonDocument.Parse(statusBody);
                var status = statusDoc.RootElement.TryGetProperty("status", out var statusElement) ? statusElement.GetString() : null;

                if (status == "pending")
                {
                    continue;
                }

                if (status == "success")
                {
                    var timestamp = statusDoc.RootElement.TryGetProperty("timestamp", out var tsElement) ? tsElement.GetString() : null;
                    var originalUrl = statusDoc.RootElement.TryGetProperty("original_url", out var origElement) ? origElement.GetString() : realUrl;
                    if (!string.IsNullOrWhiteSpace(timestamp))
                    {
                        return $"https://web.archive.org/web/{timestamp}id_/{originalUrl}";
                    }
                }

                logger.LogWarning("Wayback Machine save-page job for {Url} ended without success: {Body}", realUrl, statusBody);
                return null;
            }

            logger.LogWarning("Wayback Machine save-page job for {Url} did not complete within {Attempts} polls", realUrl, MaxPollAttempts);
            return null;
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(ex, "Wayback Machine on-demand capture failed for {Url}", realUrl);
            return null;
        }
    }

    private static async Task<string?> TryGetLatestSnapshotAsync(
        HttpClient archiveClient,
        string realUrl,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        try
        {
            var cdxUrl = $"{CdxEndpoint}?url={Uri.EscapeDataString(realUrl)}&output=json&limit=-5&filter=statuscode:200";
            using var response = await archiveClient.GetAsync(cdxUrl, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(body);
            var rows = doc.RootElement;
            if (rows.ValueKind != JsonValueKind.Array || rows.GetArrayLength() < 2)
            {
                // Row 0 is the CDX header; fewer than 2 rows means no real capture exists.
                return null;
            }

            var lastRow = rows[rows.GetArrayLength() - 1];
            var timestamp = lastRow[1].GetString();
            var originalUrl = lastRow[2].GetString();
            if (string.IsNullOrWhiteSpace(timestamp) || string.IsNullOrWhiteSpace(originalUrl))
            {
                return null;
            }

            return $"https://web.archive.org/web/{timestamp}id_/{originalUrl}";
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(ex, "Wayback Machine CDX lookup failed for {Url}", realUrl);
            return null;
        }
    }
}
