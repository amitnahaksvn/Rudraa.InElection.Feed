using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Application.ErrorLogs.Dtos;
using Application.FilteredArticles.Commands.DeleteFilteredArticles;
using Application.FilteredArticles.Dtos;
using Application.FilteredArticles.Queries.GetFilteredArticleCategories;
using Application.FilteredArticles.Queries.GetFilteredArticleProviders;
using Application.FilteredArticles.Queries.GetFilteredArticles;
using Domain.Enums;
using WebPlatform.Infrastructure;
using WebPlatform.Options;

namespace WebPlatform.Endpoints;

/// <summary>Backs the Filtered Articles admin page: a paged, newest-first log of articles excluded by the political-category allowlist (<c>NewsFilterOptions</c>), filterable by provider/type/category, plus per-row and multi-select delete.</summary>
public sealed class FilteredArticles : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        var group = groupBuilder.MapGroup("api/filtered-articles");

        group.MapGet("", GetFilteredArticles);
        group.MapGet("providers", GetProviders);
        group.MapGet("categories", GetCategories);
        group.MapDelete("", DeleteFilteredArticles);
    }

    [EndpointSummary("List filtered articles")]
    [EndpointDescription(
        "Paged, newest-first log of articles that were fetched but excluded because their " +
        "Category wasn't in the political allowlist - 'provider', 'sourceType' (Rss/Api), and " +
        "'category' optionally narrow the table, the same way the News Feed page's own filters do.")]
    public static async Task<Ok<PagedResult<FilteredArticleDto>>> GetFilteredArticles(
        ISender sender,
        IOptions<ApiOptions> apiOptions,
        int page,
        int pageSize,
        string? provider,
        ArticleSourceType? sourceType,
        string? category,
        CancellationToken cancellationToken)
    {
        var resolvedPage = page <= 0 ? 1 : page;
        var resolvedPageSize = ResolvePageSize(pageSize, apiOptions.Value);

        var result = await sender.Send(
            new GetFilteredArticlesQuery(resolvedPage, resolvedPageSize, provider, sourceType, category), cancellationToken);
        return TypedResults.Ok(result);
    }

    [EndpointSummary("Providers with filtered articles")]
    [EndpointDescription("Every distinct provider currently represented in the filtered-articles log - backs the admin page's provider filter.")]
    public static async Task<Ok<IReadOnlyList<string>>> GetProviders(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetFilteredArticleProvidersQuery(), cancellationToken);
        return TypedResults.Ok(result);
    }

    [EndpointSummary("Categories with filtered articles")]
    [EndpointDescription("Every distinct category currently represented in the filtered-articles log - backs the admin page's category filter.")]
    public static async Task<Ok<IReadOnlyList<string>>> GetCategories(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetFilteredArticleCategoriesQuery(), cancellationToken);
        return TypedResults.Ok(result);
    }

    [EndpointSummary("Delete filtered articles")]
    [EndpointDescription(
        "Removes one or more rows from the filtered-articles log by id - used by both the admin " +
        "page's per-row delete button and its multi-select bulk delete, the same request shape " +
        "either way. A hard delete, since these are diagnostic records, not real articles. Returns " +
        "how many were actually found and deleted.")]
    public static async Task<Ok<long>> DeleteFilteredArticles(
        ISender sender, [FromBody] DeleteFilteredArticlesCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return TypedResults.Ok(result);
    }

    private static int ResolvePageSize(int requested, ApiOptions options) =>
        requested <= 0 ? options.DefaultPageSize : Math.Min(requested, options.MaxPageSize);
}
