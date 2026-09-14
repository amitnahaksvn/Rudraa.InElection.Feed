using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
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
/// Three-tier resolution, the first two talking only to archive.org (never to the blocked target
/// directly):
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
/// 3. Falls back to the last snapshot URL either tier above resolved successfully for this same
///    real URL, valid for <see cref="CacheTtl"/> - added after confirming live that archive.org
///    (not just CDX specifically) has real multi-minute full outages several times in one day, well
///    beyond what a short retry can ride out. Without this, a feed whose Save-Page-Now daily quota
///    is already exhausted (making CDX its *primary* path for the rest of the day - see
///    <see cref="TryGetLatestSnapshotAsync"/>'s own doc comment) falls all the way through to the
///    blocked direct fetch on every single outage overlap, even though a perfectly good snapshot
///    URL was resolved just one crawl cycle earlier. Checked first in-process
///    (<see cref="LastKnownGoodSnapshots"/>), then in the Mongo-persisted copy
///    (<see cref="_persistentCache"/>, set once at startup via <see cref="Initialize"/>) if the
///    in-memory one has nothing - the persisted copy is what survives an actual process restart,
///    which an in-memory-only cache cannot.
/// Only if the cache is also empty does this fall back to the real URL directly (identical to
/// today's behavior before any of this existed - no worse than that baseline).
///
/// Never throws: every failure mode (missing credentials, archive.org down, job never completes,
/// malformed JSON) is caught internally and degrades to the next tier, since a fetch-URL resolver
/// must not be able to crash the crawl loop that calls it.
/// </summary>
public static class WaybackMachineFeedResolver
{
    private const string SavePageNowEndpoint = "https://web.archive.org/save/";
    private const string SaveStatusEndpoint = "https://web.archive.org/save/status/";
    private const string CdxEndpoint = "https://web.archive.org/cdx/search/cdx";
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(5);
    private const int MaxPollAttempts = 6;

    // Deliberately longer than the shortest provider cron seen using this resolver (30 minutes,
    // IndianExpress/Organiser/ThePrint) so one skipped cycle during an archive.org outage still has
    // a cached fallback to use, without serving a snapshot so old it stops being useful.
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(45);
    private static readonly ConcurrentDictionary<string, (string SnapshotUrl, DateTimeOffset ExpiresAt)> LastKnownGoodSnapshots = new();

    // Set once at startup (see RssService/Program.cs) - an in-memory-only cache is wiped by every
    // process restart, and this app got restarted far more often than a normal production deployment
    // cadence while these Wayback fixes were being rolled out one provider at a time, which is
    // exactly what kept the in-memory cache from ever getting a chance to help. Persisting to Mongo
    // means the cache survives a restart, not just the current process's own uptime. Nullable/no-op
    // when unset (e.g. in unit tests, which construct providers directly without calling Initialize)
    // - degrades to the in-memory-only behavior from before this existed.
    private static IMongoCollection<BsonDocument>? _persistentCache;

    public static void Initialize(IMongoCollection<BsonDocument> persistentCache) => _persistentCache = persistentCache;

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
                await CacheSnapshotAsync(realUrl, freshSnapshotUrl, logger, cancellationToken);
                return freshSnapshotUrl;
            }
        }

        var latestSnapshotUrl = await TryGetLatestSnapshotAsync(archiveClient, realUrl, logger, cancellationToken);
        if (latestSnapshotUrl is not null)
        {
            await CacheSnapshotAsync(realUrl, latestSnapshotUrl, logger, cancellationToken);
            return latestSnapshotUrl;
        }

        if (LastKnownGoodSnapshots.TryGetValue(realUrl, out var cached) && cached.ExpiresAt > DateTimeOffset.UtcNow)
        {
            logger.LogWarning("Wayback Machine resolution for {Url} failed this cycle - serving last known good snapshot from in-memory cache", realUrl);
            return cached.SnapshotUrl;
        }

        var persisted = await TryGetPersistedSnapshotAsync(realUrl, logger, cancellationToken);
        if (persisted is not null)
        {
            logger.LogWarning("Wayback Machine resolution for {Url} failed this cycle - serving last known good snapshot from the persistent cache (in-memory cache empty, likely a recent restart)", realUrl);
            // Warms the in-memory cache too, so subsequent failures within this same process don't
            // need a Mongo round trip until this entry's own TTL expires.
            LastKnownGoodSnapshots[realUrl] = persisted.Value;
            return persisted.Value.SnapshotUrl;
        }

        return realUrl;
    }

    private static async Task CacheSnapshotAsync(string realUrl, string snapshotUrl, ILogger logger, CancellationToken cancellationToken)
    {
        var expiresAt = DateTimeOffset.UtcNow.Add(CacheTtl);
        LastKnownGoodSnapshots[realUrl] = (snapshotUrl, expiresAt);

        if (_persistentCache is null)
        {
            return;
        }

        try
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", HashUrl(realUrl));
            var update = Builders<BsonDocument>.Update
                .Set("realUrl", realUrl)
                .Set("snapshotUrl", snapshotUrl)
                .Set("expiresAt", expiresAt.UtcDateTime);
            await _persistentCache.UpdateOneAsync(filter, update, new UpdateOptions { IsUpsert = true }, cancellationToken);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            // Best-effort - a failed cache write just means this entry won't survive a restart,
            // not a failure of the resolution that just succeeded.
            logger.LogWarning(ex, "Failed to persist Wayback Machine snapshot cache entry for {Url}", realUrl);
        }
    }

    private static async Task<(string SnapshotUrl, DateTimeOffset ExpiresAt)?> TryGetPersistedSnapshotAsync(string realUrl, ILogger logger, CancellationToken cancellationToken)
    {
        if (_persistentCache is null)
        {
            return null;
        }

        try
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", HashUrl(realUrl));
            var doc = await _persistentCache.Find(filter).FirstOrDefaultAsync(cancellationToken);
            if (doc is null)
            {
                return null;
            }

            var expiresAt = new DateTimeOffset(doc["expiresAt"].ToUniversalTime(), TimeSpan.Zero);
            if (expiresAt <= DateTimeOffset.UtcNow)
            {
                return null;
            }

            return (doc["snapshotUrl"].AsString, expiresAt);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(ex, "Failed to read persisted Wayback Machine snapshot cache entry for {Url}", realUrl);
            return null;
        }
    }

    private static string HashUrl(string url) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(url))).ToLowerInvariant();

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

    private static readonly TimeSpan CdxRetryDelay = TimeSpan.FromSeconds(3);
    private const int MaxCdxAttempts = 2;

    /// <summary>
    /// CDX is a separate subsystem from archive.org's main site and Save-Page-Now API, and is
    /// observed to occasionally return a fast HTTP 503 on its own (confirmed live: web.archive.org/
    /// itself healthy and sub-second while cdx/search/cdx 503s on every query, for several
    /// unrelated URLs at once - a transient Internet Archive-side blip, not a per-URL issue) - one
    /// retry rides out that kind of short-lived degradation. This matters more than it would for a
    /// rarely-used fallback: for any feed whose Save-Page-Now quota (5 captures/URL/day, shared
    /// across every archive.org caller for that URL, not just this app - popular pages get
    /// captured by others too) is already exhausted for the day, CDX becomes the *primary* path for
    /// the rest of that day, not an occasional backstop.
    /// </summary>
    private static async Task<string?> TryGetLatestSnapshotAsync(
        HttpClient archiveClient,
        string realUrl,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        for (var attempt = 1; attempt <= MaxCdxAttempts; attempt++)
        {
            try
            {
                var cdxUrl = $"{CdxEndpoint}?url={Uri.EscapeDataString(realUrl)}&output=json&limit=-5&filter=statuscode:200";
                using var response = await archiveClient.GetAsync(cdxUrl, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    logger.LogWarning("Wayback Machine CDX lookup for {Url} returned HTTP {Status} (attempt {Attempt}/{Max})", realUrl, (int)response.StatusCode, attempt, MaxCdxAttempts);
                    if (attempt < MaxCdxAttempts)
                    {
                        await Task.Delay(CdxRetryDelay, cancellationToken);
                        continue;
                    }

                    return null;
                }

                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                using var doc = JsonDocument.Parse(body);
                var rows = doc.RootElement;
                if (rows.ValueKind != JsonValueKind.Array || rows.GetArrayLength() < 2)
                {
                    // Row 0 is the CDX header; fewer than 2 rows means no real capture exists -
                    // not worth retrying, a retry won't make a capture that doesn't exist appear.
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
                logger.LogWarning(ex, "Wayback Machine CDX lookup failed for {Url} (attempt {Attempt}/{Max})", realUrl, attempt, MaxCdxAttempts);
                if (attempt < MaxCdxAttempts)
                {
                    await Task.Delay(CdxRetryDelay, cancellationToken);
                }
            }
        }

        return null;
    }
}
