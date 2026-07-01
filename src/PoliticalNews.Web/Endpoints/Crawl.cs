using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using PoliticalNews.Application.Crawl.Commands.TriggerCrawl;
using PoliticalNews.Application.Crawl.Dtos;
using PoliticalNews.Application.Crawl.Queries.GetCrawlHistory;
using PoliticalNews.Web.Infrastructure;

namespace PoliticalNews.Web.Endpoints;

/// <summary>Manual crawl triggering and crawl run history.</summary>
public sealed class Crawl : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        var group = groupBuilder.MapGroup("api/crawl");

        group.MapPost("trigger", Trigger);
        group.MapGet("history", GetHistory);
    }

    [EndpointSummary("Trigger a crawl")]
    [EndpointDescription(
        "Runs a crawl immediately and waits for it to finish. Subject to the same distributed " +
        "lock as the scheduled worker, so a run already in progress is skipped (409) rather than run concurrently.")]
    public static async Task<Results<Ok<CrawlHistoryDto>, Conflict<CrawlHistoryDto>>> Trigger(
        ISender sender, CancellationToken cancellationToken)
    {
        var history = await sender.Send(new TriggerCrawlCommand(), cancellationToken);
        return history.WasSkipped ? TypedResults.Conflict(history) : TypedResults.Ok(history);
    }

    [EndpointSummary("Crawl run history")]
    [EndpointDescription("Most recent crawl run records, newest first.")]
    public static async Task<Ok<IReadOnlyList<CrawlHistoryDto>>> GetHistory(
        ISender sender, int count, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetCrawlHistoryQuery(count <= 0 ? 20 : count), cancellationToken);
        return TypedResults.Ok(result);
    }
}
