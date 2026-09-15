namespace Application.Options;

/// <summary>
/// Root configuration section ("SocialMediaCrawler") gating the Mongo-driven Social pipeline
/// (<see cref="Application.Services.SocialMediaIngestionService"/> and every
/// <c>SocialMediaSource</c> document, e.g. the seeded YouTube channels) - the same kind of global
/// kill switch <see cref="NewsCrawlerOptions.Enabled"/>/<see cref="NewsApiCrawlerOptions.Enabled"/>
/// already give their own pipelines. Unlike those two, a disabled state here also sweeps every
/// already-registered <c>social-media-*</c> Hangfire recurring job (see
/// <see cref="Infrastructure.Scheduling.HangfireRecurringJobRegistrar.SeedAndRegisterSocialMediaRecurringJobsAsync"/>),
/// since flipping this flag off is the only way to stop a source that's already seeded in Mongo -
/// there's no live per-source management endpoint yet, unlike RSS/API's Provider Management page.
/// </summary>
public sealed class SocialMediaCrawlerOptions
{
    public const string SectionName = "SocialMediaCrawler";

    public bool Enabled { get; set; } = true;
}
