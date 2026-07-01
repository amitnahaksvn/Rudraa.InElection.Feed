using System.Net;

namespace PoliticalNews.Tests.TestSupport;

/// <summary>Routes requests to canned responses by exact URL, for exercising HTTP-calling code without the network.</summary>
public sealed class StubHttpMessageHandler : HttpMessageHandler
{
    private readonly Dictionary<string, string> _responsesByUrl;

    public StubHttpMessageHandler(Dictionary<string, string> responsesByUrl)
    {
        _responsesByUrl = responsesByUrl;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var url = request.RequestUri!.ToString();
        if (_responsesByUrl.TryGetValue(url, out var body))
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(body)
            });
        }

        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
    }
}

public sealed class StubHttpClientFactory : IHttpClientFactory
{
    private readonly HttpMessageHandler _handler;
    private readonly TimeSpan _timeout;

    public StubHttpClientFactory(HttpMessageHandler handler, TimeSpan? timeout = null)
    {
        _handler = handler;
        _timeout = timeout ?? TimeSpan.FromSeconds(100);
    }

    public HttpClient CreateClient(string name) => new(_handler) { Timeout = _timeout };
}

/// <summary>Simulates a hung/dead server: never responds until the caller's own token cancels the wait.</summary>
public sealed class HangingHttpMessageHandler : HttpMessageHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
        throw new InvalidOperationException("unreachable");
    }
}
