using Mediator;
using Application.Abstractions;

namespace Application.Providers.Commands.DeleteCrawlFeed;

/// <summary>Removes one feed/endpoint - the delete button on each feed row in the Provider Management page.</summary>
public sealed record DeleteCrawlFeedCommand(string Id) : IRequest<bool>;

public sealed class DeleteCrawlFeedCommandHandler : IRequestHandler<DeleteCrawlFeedCommand, bool>
{
    private readonly ICrawlFeedRepository _feeds;

    public DeleteCrawlFeedCommandHandler(ICrawlFeedRepository feeds)
    {
        _feeds = feeds;
    }

    public async ValueTask<bool> Handle(DeleteCrawlFeedCommand request, CancellationToken cancellationToken) =>
        await _feeds.DeleteAsync(request.Id, cancellationToken);
}
