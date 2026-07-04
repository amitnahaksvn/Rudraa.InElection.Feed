using System.Security.Cryptography;
using System.Text;
using Hangfire.Dashboard;

namespace Web.Infrastructure;

/// <summary>
/// Hangfire's dashboard ships with no authorization of its own - <c>UseHangfireDashboard()</c>
/// with no filter serves it to anyone who can reach the route. This requires HTTP Basic Auth
/// against <c>Api:HangfireDashboardUsername</c>/<c>Api:HangfireDashboardPassword</c>; if either is
/// blank, every request is denied (fails closed) rather than the dashboard silently being open to
/// all, which is what happened in production before this filter existed.
/// </summary>
public sealed class HangfireBasicAuthAuthorizationFilter(string username, string password) : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            return false;
        }

        var httpContext = context.GetHttpContext();
        var header = httpContext.Request.Headers.Authorization.ToString();

        if (string.IsNullOrEmpty(header) || !header.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
        {
            Challenge(httpContext);
            return false;
        }

        string decoded;
        try
        {
            decoded = Encoding.UTF8.GetString(Convert.FromBase64String(header["Basic ".Length..]));
        }
        catch (FormatException)
        {
            Challenge(httpContext);
            return false;
        }

        var separatorIndex = decoded.IndexOf(':');
        if (separatorIndex < 0)
        {
            Challenge(httpContext);
            return false;
        }

        var providedUsername = decoded[..separatorIndex];
        var providedPassword = decoded[(separatorIndex + 1)..];

        if (FixedTimeEquals(providedUsername, username) && FixedTimeEquals(providedPassword, password))
        {
            return true;
        }

        Challenge(httpContext);
        return false;
    }

    private static void Challenge(HttpContext httpContext)
    {
        httpContext.Response.Headers.WWWAuthenticate = "Basic realm=\"Hangfire Dashboard\"";
        httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
    }

    // Ordinary string equality short-circuits on the first mismatched byte, letting an attacker
    // infer the correct credential length/prefix via response-time differences - irrelevant at the
    // scale this dashboard sees, but a fixed-time comparison costs nothing here and is the correct
    // default for any credential check.
    private static bool FixedTimeEquals(string a, string b) =>
        CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(a), Encoding.UTF8.GetBytes(b));
}
