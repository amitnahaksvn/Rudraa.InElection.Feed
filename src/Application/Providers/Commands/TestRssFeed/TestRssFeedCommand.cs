using Mediator;
using Application.Abstractions;
using Application.Options;
using Application.Providers.Dtos;

namespace Application.Providers.Commands.TestRssFeed;

/// <summary>On-demand connectivity/content test for one already-configured RSS feed, triggered from the Provider Management page's Test button. Never persists anything - it's a pure diagnostic fetch, not a real crawl run.</summary>
public sealed record TestRssFeedCommand(string FeedId) : IRequest<ProviderTestResultDto>;

public sealed class TestRssFeedCommandHandler : IRequestHandler<TestRssFeedCommand, ProviderTestResultDto>
{
    private readonly IEnumerable<IRssProvider> _providers;
    private readonly ICrawlFeedRepository _feeds;

    public TestRssFeedCommandHandler(IEnumerable<IRssProvider> providers, ICrawlFeedRepository feeds)
    {
        _providers = providers;
        _feeds = feeds;
    }

    public async ValueTask<ProviderTestResultDto> Handle(TestRssFeedCommand request, CancellationToken cancellationToken)
    {
        var feed = await _feeds.GetByIdAsync(request.FeedId, cancellationToken);
        if (feed is null)
        {
            return ProviderTestResultDto.NotFound($"No feed found with id '{request.FeedId}'.");
        }

        var providerImpl = _providers.FirstOrDefault(p => p.Name == feed.Provider);
        if (providerImpl is null)
        {
            return ProviderTestResultDto.NotFound($"No RSS provider registered with name '{feed.Provider}'.");
        }

        // Forced enabled regardless of the feed's own configured value - "Test" is an explicit,
        // one-off diagnostic action, so a feed that's currently disabled should still be testable.
        var testFeed = new RssFeedOptions
        {
            Name = feed.Name,
            Url = feed.Url,
            Category = feed.Category,
            Language = feed.Language,
            Enabled = true,
            DefaultImageUrl = feed.DefaultImageUrl
        };

        var results = await providerImpl.FetchAllFeedsAsync([testFeed], cancellationToken);
        var result = results.FirstOrDefault();
        return result is null
            ? ProviderTestResultDto.NotFound("The provider returned no result for this feed.")
            : ProviderTestResultDto.FromFeedResult(result);
    }
}
