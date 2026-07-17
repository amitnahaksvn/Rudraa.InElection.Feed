using Application.Models;
using Application.Options;

namespace Application.Abstractions;

/// <summary>
/// A single JSON news-API integration (NewsAPI.org, GNews, TheNewsAPI, Currents, Mediastack,
/// NewsData.io). The <see cref="IRssProvider"/> counterpart for polled REST APIs -
/// implementations only know how to call their own endpoints and normalize each response; they
/// never persist anything themselves.
/// </summary>
public interface INewsApiProvider
{
    /// <summary>Provider key, must match a <see cref="NewsApiProviderOptions.Name"/> entry in configuration.</summary>
    string Name { get; }

    /// <summary>Calls every enabled endpoint under this provider once and normalizes each response - one result per endpoint, mirroring <see cref="IRssProvider.FetchAllFeedsAsync"/>.</summary>
    Task<IReadOnlyList<ApiFetchResult>> FetchAllEndpointsAsync(NewsApiProviderOptions options, CancellationToken cancellationToken);
}
