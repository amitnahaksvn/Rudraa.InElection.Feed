using Microsoft.Extensions.Logging;
using Application.Abstractions;
using Application.Validators;
using Domain.Entities;

namespace Infrastructure.Seed;

/// <summary>
/// Idempotent Phase 1 seed for the <see cref="FeedSource"/> collection - inserts the official PIB
/// press-release feed if (and only if) a document with that <c>SourceCode</c> doesn't already
/// exist, so re-running this on every startup never creates duplicates. Every later feed
/// (AajTak, ABP, Google News, YouTube, ...) is added the same way this one was: a document insert,
/// not a code change - this seeder exists solely to bootstrap the very first one.
/// </summary>
public sealed class FeedSourceSeeder
{
    // Verified working during discovery: ModId=6 (press releases), Mod=1, reg=3, lang=1 (English).
    // The commonly-quoted "ModId=6&Lang=1&Regid=1" pattern returns HTTP 200 but an empty channel
    // (0 items) - it looks plausible but isn't real content, so it's deliberately not used here.
    private const string PibFeedUrl = "https://pib.gov.in/RssMain.aspx?ModId=6&Mod=1&reg=3&lang=1";

    private readonly IFeedSourceRepository _feedSourceRepository;
    private readonly FeedSourceValidator _validator;
    private readonly ILogger<FeedSourceSeeder> _logger;

    public FeedSourceSeeder(IFeedSourceRepository feedSourceRepository, FeedSourceValidator validator, ILogger<FeedSourceSeeder> logger)
    {
        _feedSourceRepository = feedSourceRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        var existing = await _feedSourceRepository.GetBySourceCodeAsync("PIB", cancellationToken);
        if (existing is not null)
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        var feedSource = new FeedSource
        {
            SourceCode = "PIB",
            SourceName = "Press Information Bureau",
            FeedName = "Press Releases",
            FeedUrl = PibFeedUrl,
            Country = "India",
            Language = "en",
            Category = "Government",
            Priority = 1,
            IsActive = true,
            FetchIntervalMinutes = 5,
            TimeoutSeconds = 60,
            CreatedOn = now,
            UpdatedOn = now
        };

        var validation = _validator.Validate(feedSource);
        if (!validation.IsValid)
        {
            _logger.LogError("PIB seed FeedSource failed validation: {Errors}", validation);
            return;
        }

        var id = await _feedSourceRepository.InsertAsync(feedSource, cancellationToken);
        _logger.LogInformation("Seeded FeedSource 'PIB' ({Id})", id);
    }

    /// <summary>
    /// One-time cleanup for this Phase 1 pipeline's own PIB document: PIB has since gained a proper
    /// catalog entry (<c>PibRssProvider</c>, routed through <see cref="Infrastructure.RssProviders.WaybackMachineFeedResolver"/>
    /// to survive pib.gov.in's connection timeouts from this app's outbound IP), which is the
    /// spec-tolerance handling this <see cref="FeedSource"/> pipeline was never meant to cover (see
    /// this class's own doc comment: "a publisher that needs real spec-tolerance handling...still
    /// belongs in the file-based provider system, not here"). Left active, this document keeps
    /// registering its own independent "dynamic-feed-PIB" Hangfire job forever, invisible to and
    /// unaffected by the Provider Management page's PIB enable/disable toggle (which only knows
    /// about the catalog provider's own "news-crawl-PIB::India" job) - exactly the "I disabled PIB
    /// but it's still running in Hangfire" symptom this fixes. Deactivating it here is enough:
    /// <see cref="HangfireRecurringJobRegistrar.SeedAndRegisterDynamicFeedRecurringJobsAsync"/>'s
    /// own stale-job sweep removes "dynamic-feed-PIB" on the very next startup once this document's
    /// <see cref="FeedSource.IsActive"/> is false, the same way it already removes any other
    /// deactivated/deleted FeedSource's job.
    /// </summary>
    public Task RetireSupersededFeedsAsync(CancellationToken cancellationToken) =>
        _feedSourceRepository.DeactivateBySourceCodeAsync("PIB", cancellationToken);
}
