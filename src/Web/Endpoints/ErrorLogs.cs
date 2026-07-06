using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;
using Application.ErrorLogs.Commands.SetErrorLogResolved;
using Application.ErrorLogs.Dtos;
using Application.ErrorLogs.Queries.GetErrorLogById;
using Application.ErrorLogs.Queries.GetErrorLogs;
using Web.Infrastructure;
using Web.Options;

namespace Web.Endpoints;

/// <summary>Backs the error-monitor page: paged/filterable list of persisted <c>ErrorLog</c> rows, single-error detail, and marking a row resolved/unresolved.</summary>
public sealed class ErrorLogs : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        var group = groupBuilder.MapGroup("api/errors");

        group.MapGet("", GetList);
        group.MapGet("{id}", GetById);
        group.MapPatch("{id}/resolved", SetResolved);
    }

    [EndpointSummary("List errors")]
    [EndpointDescription(
        "Paged, filterable error-log rows for the error-monitor UI - unresolved rows first, newest " +
        "first within each group. All filter parameters are optional.")]
    public static async Task<Ok<PagedResult<ErrorLogSummaryDto>>> GetList(
        ISender sender,
        IOptions<ApiOptions> apiOptions,
        int page,
        int pageSize,
        bool? isResolved,
        string? provider,
        string? country,
        string? source,
        string? search,
        CancellationToken cancellationToken)
    {
        var resolvedPage = page <= 0 ? 1 : page;
        var resolvedPageSize = ResolvePageSize(pageSize, apiOptions.Value);

        var result = await sender.Send(
            new GetErrorLogsQuery(resolvedPage, resolvedPageSize, isResolved, provider, country, source, search),
            cancellationToken);
        return TypedResults.Ok(result);
    }

    [EndpointSummary("Get one error's full detail")]
    [EndpointDescription("Every field for a single error, including stack trace and request/response bodies - only fetched when a row is expanded.")]
    public static async Task<Results<Ok<ErrorLogDetailDto>, NotFound>> GetById(
        ISender sender, string id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetErrorLogByIdQuery(id), cancellationToken);
        return result is null ? TypedResults.NotFound() : TypedResults.Ok(result);
    }

    [EndpointSummary("Mark an error resolved or unresolved")]
    [EndpointDescription("Body: { \"resolved\": true }. Used by the inline toggle in both the list row and the expanded detail view.")]
    public static async Task<Results<Ok, NotFound>> SetResolved(
        ISender sender, string id, SetResolvedRequest request, CancellationToken cancellationToken)
    {
        var found = await sender.Send(new SetErrorLogResolvedCommand(id, request.Resolved), cancellationToken);
        return found ? TypedResults.Ok() : TypedResults.NotFound();
    }

    private static int ResolvePageSize(int requested, ApiOptions options) =>
        requested <= 0 ? options.DefaultPageSize : Math.Min(requested, options.MaxPageSize);
}

public sealed record SetResolvedRequest(bool Resolved);
