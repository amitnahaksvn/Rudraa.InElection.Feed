using System.Text.RegularExpressions;
using Mediator;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Infrastructure.Cosmos;

/// <summary>
/// Retries a request when it fails with Azure Cosmos DB's Mongo API "Request rate is large" (429)
/// throttling response - a real, expected condition on Cosmos's RU-based throughput model, not a
/// bug: this app's own read/write volume can legitimately exceed a given account's provisioned
/// RU/s, especially under a burst of concurrent requests (e.g. the News Feed page firing several
/// parallel queries on load - confirmed live: GetNewsCountriesQuery and GetNewsFeedCountQuery both
/// hit this back to back against this app's real Cosmos DB account). The MongoDB C# driver's own
/// built-in retryable-reads/writes doesn't retry this - Cosmos returns it as a generic
/// <see cref="MongoCommandException"/> (code 16500), not one of the driver's own recognized
/// "retryable" error labels - so this behaviour fills that gap centrally in the Mediator pipeline,
/// rather than wrapping every individual repository call.
///
/// Lives in Infrastructure, not alongside the other <c>IPipelineBehavior</c>s in
/// <c>Application/Common/Behaviours</c>, specifically because it needs <c>MongoDB.Driver</c> to
/// catch <see cref="MongoCommandException"/> - Application has zero Mongo dependency by design
/// (see CLAUDE.md's Clean Architecture note), so a Mongo/Cosmos-specific behaviour has to be
/// registered from <c>AddInfrastructure</c> instead. Registered after Application's own
/// Logging/UnhandledException/Validation/Performance behaviours (<c>AddInfrastructure</c> always
/// runs after <c>AddApplication</c> in every host's Program.cs), which - since each pipeline
/// behaviour registration wraps one layer further in - places this as the innermost layer, closest
/// to the actual handler: only a retries-exhausted failure ever reaches
/// <c>UnhandledExceptionBehaviour</c>'s own error log, and <c>PerformanceBehaviour</c>'s measured
/// time reflects true end-to-end latency including any retry backoff.
///
/// Honors Cosmos's own RetryAfterMs hint (embedded in the error message text, not a structured
/// field on the exception) when present, falling back to a short fixed backoff otherwise.
/// </summary>
public sealed class CosmosThrottleRetryBehaviour<TMessage, TResponse> : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    private const int MaxAttempts = 4;
    private static readonly Regex RetryAfterMsPattern = new(@"RetryAfterMs=(\d+)", RegexOptions.Compiled);

    private readonly ILogger<CosmosThrottleRetryBehaviour<TMessage, TResponse>> _logger;

    public CosmosThrottleRetryBehaviour(ILogger<CosmosThrottleRetryBehaviour<TMessage, TResponse>> logger)
    {
        _logger = logger;
    }

    public async ValueTask<TResponse> Handle(
        TMessage message, MessageHandlerDelegate<TMessage, TResponse> next, CancellationToken cancellationToken)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                return await next(message, cancellationToken);
            }
            catch (MongoCommandException ex) when (attempt < MaxAttempts && IsCosmosThrottling(ex))
            {
                var delay = ExtractRetryAfter(ex.Message) ?? TimeSpan.FromMilliseconds(500 * attempt);
                _logger.LogWarning(
                    "Cosmos DB throttled {RequestName} (attempt {Attempt}/{MaxAttempts}) - retrying in {Delay}",
                    typeof(TMessage).Name, attempt, MaxAttempts, delay);
                await Task.Delay(delay, cancellationToken);
            }
        }
    }

    private static bool IsCosmosThrottling(MongoCommandException ex) =>
        ex.Message.Contains("TooManyRequests (429)", StringComparison.OrdinalIgnoreCase);

    private static TimeSpan? ExtractRetryAfter(string message)
    {
        var match = RetryAfterMsPattern.Match(message);
        return match.Success && int.TryParse(match.Groups[1].Value, out var ms) ? TimeSpan.FromMilliseconds(ms) : null;
    }
}
