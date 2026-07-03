using Application.Models;

namespace Application.Abstractions;

/// <summary>
/// Provider-agnostic email notification service. Business logic (the crawl orchestrators) depends
/// only on this interface, never on a concrete provider SDK type - swapping the implementation
/// (Resend today; SendGrid/SES/SMTP/Azure Communication Services later) never requires touching a
/// caller. Every method is guaranteed to never throw: implementations must catch and log their own
/// failures internally, since a monitoring-alert send failing must never itself take down the
/// crawler it was trying to report on.
/// </summary>
public interface IEmailService
{
    /// <summary>Sends one error alert email for a single failure.</summary>
    Task SendErrorAsync(ErrorNotification notification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends one email covering every error from a single execution (e.g. one crawl run) - a
    /// summary table of all of them plus a detailed section per error - rather than one email per
    /// failure, so a provider with many failing feeds in one run doesn't flood the inbox.
    /// </summary>
    Task SendErrorSummaryAsync(
        IReadOnlyList<ErrorNotification> notifications,
        string executionContext,
        CancellationToken cancellationToken = default);

    Task SendWarningAsync(string subject, string message, CancellationToken cancellationToken = default);

    Task SendInformationAsync(string subject, string message, CancellationToken cancellationToken = default);

    Task SendSuccessAsync(string subject, string message, CancellationToken cancellationToken = default);
}
