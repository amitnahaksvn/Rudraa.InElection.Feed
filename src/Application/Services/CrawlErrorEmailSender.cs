using Microsoft.Extensions.Logging;
using Application.Abstractions;
using Application.Models;

namespace Application.Services;

/// <summary>
/// Shared "send the error-summary email, never let a failure here fail the run" wrapper used by
/// every crawler orchestrator - <see cref="NewsCrawlerOrchestrator"/> (RSS) and
/// <see cref="NewsApiCrawlerOrchestrator"/> (JSON APIs) alike. <see cref="IEmailService"/>
/// implementations already guarantee they never throw, but catching here too is cheap, defensive
/// insurance against that contract - a monitoring-alert failure must never fail the crawl run it
/// was reporting on.
/// </summary>
internal static class CrawlErrorEmailSender
{
    public static async Task SendIfAnyAsync(
        IEmailService emailService,
        IReadOnlyList<ErrorNotification> errors,
        string executionContext,
        ILogger logger,
        string historyId,
        CancellationToken cancellationToken)
    {
        if (errors.Count == 0)
        {
            return;
        }

        try
        {
            await emailService.SendErrorSummaryAsync(errors, executionContext, cancellationToken);
        }
        catch (Exception emailEx)
        {
            logger.LogError(emailEx, "[{RunId}] Failed to send crawl error-summary email", historyId);
        }
    }
}
