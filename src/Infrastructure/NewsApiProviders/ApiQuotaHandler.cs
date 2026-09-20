using System.Net;
using Application.Abstractions;

namespace Infrastructure.NewsApiProviders;

/// <summary>
/// Sits <em>inside</em> the news-API client's Polly retry policy, so every actual HTTP attempt -
/// including each retry - is counted against the provider's daily budget, not just each logical
/// fetch. Requests without a limit set (<see cref="ProviderKey"/>/<see cref="DailyLimitKey"/>) pass
/// straight through. Once the budget is spent it answers 429 locally without touching the network
/// (429 isn't a transient error to the retry policy, so it isn't retried).
/// </summary>
public sealed class ApiQuotaHandler : DelegatingHandler
{
    public static readonly HttpRequestOptionsKey<string> ProviderKey = new("quota.provider");
    public static readonly HttpRequestOptionsKey<int> DailyLimitKey = new("quota.dailyLimit");

    /// <summary>Header set on the locally-generated 429, so callers can tell "our own daily budget is spent" from a real 429 sent by the API.</summary>
    public const string ExceededHeader = "X-Local-Quota-Exceeded";

    private readonly IApiRequestQuota _quota;

    public ApiQuotaHandler(IApiRequestQuota quota)
    {
        _quota = quota;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Options.TryGetValue(ProviderKey, out var provider)
            && request.Options.TryGetValue(DailyLimitKey, out var limit)
            && !await _quota.TryConsumeAsync(provider, limit, cancellationToken))
        {
            var blocked = new HttpResponseMessage(HttpStatusCode.TooManyRequests)
            {
                RequestMessage = request,
                Content = new StringContent($"Daily request limit ({limit}) reached for {provider}; no request was sent.")
            };
            blocked.Headers.TryAddWithoutValidation(ExceededHeader, "1");
            return blocked;
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
