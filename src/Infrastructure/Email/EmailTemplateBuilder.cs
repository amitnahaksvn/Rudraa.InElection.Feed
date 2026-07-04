using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Application.Models;

namespace Infrastructure.Email;

/// <summary>
/// Builds the HTML bodies for every monitoring-alert email, reading the shared chrome from
/// <c>Templates/Layout.html</c> on disk (see that file's own header comment) and generating only
/// the dynamic inner content (stat badges, context/exception/tracing panels, tables) here in code
/// - the two are split deliberately so branding/style tweaks (colors, footer copy, fonts) never
/// need a C# change, only an edit to the .html file. Lives in Infrastructure (not Application)
/// specifically because "how a notification renders" is a provider/presentation concern, unlike
/// <see cref="ErrorNotification"/> itself which is provider-agnostic business data.
/// </summary>
public sealed class EmailTemplateBuilder
{
    private const int MaxFieldLength = 4000;

    private static readonly string LayoutPath = Path.Combine(AppContext.BaseDirectory, "Email", "Templates", "Layout.html");
    private const string FallbackLayout =
        "<!doctype html><html><body style=\"font-family:sans-serif;\">" +
        "<div style=\"border-top:5px solid {{AccentColor}};padding:16px;background:#111827;color:#fff;\">" +
        "<b>{{Icon}} {{HeaderTitle}}</b><br/>{{HeaderSubtitle}}</div>" +
        "<div style=\"padding:16px;\">{{Body}}</div>" +
        "<div style=\"padding:12px;color:#999;font-size:11px;\">{{FooterNote}}</div>" +
        "</body></html>";

    private readonly ILogger<EmailTemplateBuilder> _logger;
    private readonly Lazy<string> _layoutHtml;

    public EmailTemplateBuilder(ILogger<EmailTemplateBuilder> logger)
    {
        _logger = logger;
        _layoutHtml = new Lazy<string>(LoadLayout);
    }

    private static readonly Regex HtmlCommentRegex = new("<!--.*?-->", RegexOptions.Singleline | RegexOptions.Compiled);

    private string LoadLayout()
    {
        try
        {
            // Layout.html's own leading comment documents the {{Placeholder}} tokens for whoever
            // edits the file - stripped here (comments never render in an email client anyway) so
            // that literal "{{...}}" text inside the comment isn't itself replaced below, and so
            // the comment never ships in the actual sent email's HTML source.
            var raw = File.ReadAllText(LayoutPath);
            return HtmlCommentRegex.Replace(raw, string.Empty);
        }
        catch (Exception ex)
        {
            // A missing/unreadable template file must never stop an alert email from going out -
            // fall back to a minimal inline layout instead.
            _logger.LogWarning(ex, "Could not read email layout template at {Path} - using built-in fallback layout", LayoutPath);
            return FallbackLayout;
        }
    }

    private static readonly (string Icon, string Color, string ColorSoft) Critical = ("🚨", "#DC2626", "#FEE2E2");
    private static readonly (string Icon, string Color, string ColorSoft) Warning = ("⚠️", "#D97706", "#FEF3C7");
    private static readonly (string Icon, string Color, string ColorSoft) Info = ("ℹ️", "#2563EB", "#DBEAFE");
    private static readonly (string Icon, string Color, string ColorSoft) Success = ("✅", "#16A34A", "#DCFCE7");

    public (string Subject, string Html) BuildError(ErrorNotification n)
    {
        var subject = $"🚨 InElection Monitoring Alert - {n.ApplicationName}: {n.Operation} failed" + (n.Provider is { } p ? $" ({p})" : "");

        var body = new StringBuilder();
        body.Append(StatStrip([
            ("Status", "FAILED", Critical.Color),
            ("Provider", n.Provider ?? "-", "#374151"),
            ("Operation", n.Operation, "#374151")
        ]));
        body.Append(ErrorSections(n));

        var html = Render(Critical, "InElection Monitoring Alert", HeaderBadges(n.Environment, n.ApplicationName), body.ToString());
        return (subject, html);
    }

    public (string Subject, string Html) BuildErrorSummary(IReadOnlyList<ErrorNotification> notifications, string executionContext)
    {
        var subject = $"🚨 InElection Monitoring Alert - {executionContext}: {notifications.Count} error(s)";
        var first = notifications[0];
        var providersAffected = notifications.Select(x => x.Provider ?? x.Operation).Distinct().Count();

        var body = new StringBuilder();
        body.Append(StatStrip([
            ("Execution", executionContext, "#374151"),
            ("Total Errors", notifications.Count.ToString(), Critical.Color),
            ("Providers Affected", providersAffected.ToString(), "#374151")
        ]));

        body.Append(PanelOpen("📋", "Summary"));
        body.Append(SummaryTable(notifications));
        body.Append(PanelClose());

        body.Append("""<h3 style="margin:24px 0 4px;font-size:13px;color:#374151;text-transform:uppercase;letter-spacing:0.4px;">Error Details</h3>""");
        for (var i = 0; i < notifications.Count; i++)
        {
            var n = notifications[i];
            body.Append($"""<div style="margin:14px 0 6px;font-size:13px;font-weight:700;color:#111827;">#{i + 1} &middot; {Encode(n.Provider ?? n.Operation)}{(n.FeedOrApiName is { } f ? $" / {Encode(f)}" : "")}</div>""");
            body.Append(ErrorSections(n));
        }

        var html = Render(Critical, "InElection Monitoring Alert", HeaderBadges(first.Environment, first.ApplicationName), body.ToString());
        return (subject, html);
    }

    public (string Subject, string Html) BuildSimple(string kind, string subject, string message)
    {
        var theme = kind switch
        {
            "Warning" => Warning,
            "Information" => Info,
            "Success" => Success,
            _ => Info
        };
        var emojiPrefix = kind switch { "Warning" => "⚠️", "Success" => "✅", _ => "ℹ️" };
        var fullSubject = $"{emojiPrefix} InElection {kind} - {subject}";

        var body = new StringBuilder();
        body.Append(PanelOpen(theme.Icon, Encode(subject)));
        body.Append($"""<p style="margin:0;font-size:14px;color:#374151;line-height:1.7;white-space:pre-wrap;">{Encode(message)}</p>""");
        body.Append(PanelClose());

        var html = Render(theme, $"InElection {kind}", "", body.ToString());
        return (fullSubject, html);
    }

    // ---- layout plumbing ----

    private string Render((string Icon, string Color, string ColorSoft) theme, string headerTitle, string headerSubtitleHtml, string bodyHtml) =>
        _layoutHtml.Value
            .Replace("{{AccentColor}}", theme.Color)
            .Replace("{{AccentColorSoft}}", theme.ColorSoft)
            .Replace("{{Icon}}", theme.Icon)
            .Replace("{{HeaderTitle}}", Encode(headerTitle))
            .Replace("{{HeaderSubtitle}}", headerSubtitleHtml)
            .Replace("{{Body}}", bodyHtml)
            .Replace("{{FooterNote}}", Encode($"Generated {DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm:ss} UTC"));

    private static string HeaderBadges(string environment, string applicationName) => $"""
        <span style="{BadgeStyle()}">ENV: {Encode(environment.ToUpperInvariant())}</span>
        <span style="{BadgeStyle()}margin-left:6px;">APP: {Encode(applicationName)}</span>
        """;

    private static string BadgeStyle() =>
        "display:inline-block;background:rgba(255,255,255,0.14);color:#e5e7eb;font-size:11px;font-weight:600;" +
        "padding:3px 9px;border-radius:20px;letter-spacing:0.3px;";

    private static string StatStrip((string Label, string Value, string Color)[] stats)
    {
        var sb = new StringBuilder("""<table role="presentation" style="width:100%;border-collapse:collapse;margin-bottom:20px;"><tr>""");
        foreach (var (label, value, color) in stats)
        {
            sb.Append($"""
                <td style="padding:10px 14px;background:#f9fafb;border:1px solid #eef0f2;border-radius:8px;">
                <div style="font-size:10px;color:#9aa1ab;text-transform:uppercase;letter-spacing:0.4px;">{Encode(label)}</div>
                <div style="font-size:14px;font-weight:700;color:{color};margin-top:2px;">{Encode(value)}</div>
                </td><td style="width:8px;"></td>
                """);
        }
        sb.Append("</tr></table>");
        return sb.ToString();
    }

    private static string PanelOpen(string icon, string title) => $"""
        <div style="margin-bottom:16px;border:1px solid #eef0f2;border-radius:8px;overflow:hidden;">
        <div style="background:#f9fafb;padding:10px 14px;border-bottom:1px solid #eef0f2;font-size:12px;font-weight:700;color:#374151;text-transform:uppercase;letter-spacing:0.4px;">{icon} {title}</div>
        <div style="padding:14px;">
        """;

    private static string PanelClose() => "</div></div>";

    private static string ErrorSections(ErrorNotification n)
    {
        var sb = new StringBuilder();

        sb.Append(PanelOpen("🧭", "Context"));
        sb.Append(KeyValueGrid([
            ("Date & Time (UTC)", n.OccurredAt.UtcDateTime.ToString("yyyy-MM-dd HH:mm:ss")),
            ("Provider Name", n.Provider),
            ("Feed/API Name", n.FeedOrApiName),
            ("Source URL", n.SourceUrl),
            ("Operation", n.Operation)
        ]));
        sb.Append(PanelClose());

        sb.Append(PanelOpen("💥", "Exception"));
        sb.Append(KeyValueGrid([
            ("Exception Type", n.ExceptionType),
            ("Error Message", n.ErrorMessage),
            ("Inner Exception", n.InnerException)
        ]));
        sb.Append(PanelClose());

        if (n.HttpStatusCode is not null || n.ResponseBody is not null || n.RequestUrl is not null)
        {
            sb.Append(PanelOpen("🌐", "HTTP"));
            sb.Append(KeyValueGrid([
                ("HTTP Status Code", n.HttpStatusCode?.ToString()),
                ("Request URL", n.RequestUrl)
            ]));
            if (!string.IsNullOrWhiteSpace(n.ResponseBody))
            {
                sb.Append("""<div style="font-size:11px;color:#9aa1ab;text-transform:uppercase;letter-spacing:0.4px;margin:8px 0 4px;">Response Body</div>""");
                sb.Append(CodeBlock(Truncate(n.ResponseBody)!));
            }
            sb.Append(PanelClose());
        }

        if (!string.IsNullOrWhiteSpace(n.StackTrace))
        {
            sb.Append(PanelOpen("🧵", "Stack Trace"));
            sb.Append(CodeBlock(Truncate(n.StackTrace)!));
            sb.Append(PanelClose());
        }

        sb.Append(PanelOpen("🔗", "Tracing"));
        sb.Append(KeyValueGrid([
            ("Correlation Id", n.CorrelationId),
            ("Hangfire Job Id", n.HangfireJobId),
            ("Execution Duration", n.ExecutionDuration?.ToString("g"))
        ]));
        sb.Append(PanelClose());

        return sb.ToString();
    }

    private static string KeyValueGrid((string Label, string? Value)[] pairs)
    {
        var sb = new StringBuilder("""<table role="presentation" style="width:100%;border-collapse:collapse;">""");
        foreach (var (label, value) in pairs)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }

            sb.Append("<tr>");
            sb.Append($"""<td style="padding:5px 0;font-size:11px;color:#9aa1ab;width:170px;vertical-align:top;white-space:nowrap;text-transform:uppercase;letter-spacing:0.3px;">{Encode(label)}</td>""");
            sb.Append($"""<td style="padding:5px 0;font-size:13px;color:#111827;word-break:break-word;">{Encode(value)}</td>""");
            sb.Append("</tr>");
        }
        sb.Append("</table>");
        return sb.ToString();
    }

    private static string CodeBlock(string content) => $"""
        <pre style="margin:0;white-space:pre-wrap;word-break:break-word;font-family:'SF Mono',Consolas,Menlo,monospace;font-size:12px;line-height:1.5;background:#0f172a;color:#d1e3ff;padding:12px 14px;border-radius:6px;">{Encode(content)}</pre>
        """;

    private static string SummaryTable(IReadOnlyList<ErrorNotification> notifications)
    {
        var sb = new StringBuilder("""<table role="presentation" style="width:100%;border-collapse:collapse;font-size:12px;">""");
        sb.Append("""<tr style="background:#f9fafb;">""");
        foreach (var col in new[] { "#", "Provider", "Feed/API", "Operation", "Exception", "HTTP" })
        {
            sb.Append($"""<th style="text-align:left;padding:8px 10px;border-bottom:2px solid #eef0f2;color:#6b7280;text-transform:uppercase;font-size:10px;letter-spacing:0.3px;">{col}</th>""");
        }
        sb.Append("</tr>");

        for (var i = 0; i < notifications.Count; i++)
        {
            var n = notifications[i];
            var rowBg = i % 2 == 0 ? "#ffffff" : "#fbfbfc";
            sb.Append($"""<tr style="background:{rowBg};">""");
            sb.Append(Cell($"""<span style="display:inline-block;width:7px;height:7px;border-radius:50%;background:{Critical.Color};margin-right:6px;"></span>{i + 1}"""));
            sb.Append(Cell(Encode(n.Provider ?? "-")));
            sb.Append(Cell(Encode(n.FeedOrApiName ?? "-")));
            sb.Append(Cell(Encode(n.Operation)));
            sb.Append(Cell(Encode(n.ExceptionType)));
            sb.Append(Cell(Encode(n.HttpStatusCode?.ToString() ?? "-")));
            sb.Append("</tr>");
        }
        sb.Append("</table>");
        return sb.ToString();

        static string Cell(string html) => $"""<td style="padding:8px 10px;border-bottom:1px solid #f1f2f4;color:#374151;">{html}</td>""";
    }

    private static string? Truncate(string? value) =>
        value is null ? null : value.Length > MaxFieldLength ? value[..MaxFieldLength] + "\n... [truncated]" : value;

    private static string Encode(string? value) => WebUtility.HtmlEncode(value ?? string.Empty);
}
