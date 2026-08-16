var builder = DistributedApplication.CreateBuilder(args);

// Reads a real connection string named "cosmosdb" from this AppHost's own configuration
// (user-secrets/env var), e.g. an existing Azure Cosmos DB for MongoDB account - no local Docker
// container involved. Every collection except Hangfire's own storage lives here.
var cosmosConnectionString = builder.AddConnectionString("cosmosdb");

// Hangfire's own job storage stays on a real MongoDB instance instead (see
// WebPlatform.HangfireStorageSetup's own doc comment for why) - a second, independent connection
// string resource, not the same one as cosmosConnectionString above.
var hangfireMongoConnectionString = builder.AddConnectionString("mongodb");

// Pinned (rather than Aspire's usual dynamically-assigned port) so the browser-auto-open below
// can target a known URL - also matches each project's own launchSettings.json default, so it's
// the same address whether launched through the AppHost or standalone.
const int webAppPort = 5095;
const int rssPort = 5096;
const int apiPort = 5097;

// Three independent processes, all pointed at the same Cosmos DB account and the same Hangfire
// MongoDB instance (those shared connection strings/databases are what keeps their data - and
// Hangfire job storage - unified despite running separately):
//   - WebApp: the one admin site/dashboard/read API. Never executes a crawl job itself, only
//     enqueues/manages them against the shared Hangfire storage.
//   - RssService: headless worker, owns RSS + Dynamic-feed job execution.
//   - ApiService: headless worker, owns JSON-API + Social job execution.
// Retired from the single combined Web process this used to be, and then from a brief
// two-full-copies split, before settling on this shape (see git history/CLAUDE.md) - one process
// so a pipeline's job volume can never starve another's, but only one admin site instead of
// duplicating it per pipeline.
builder.AddProject<Projects.WebApp>("webapp")
    .WithReference(cosmosConnectionString)
    .WaitFor(cosmosConnectionString)
    .WithReference(hangfireMongoConnectionString)
    .WaitFor(hangfireMongoConnectionString)
    .WithHttpEndpoint(port: webAppPort)
    .WithExternalHttpEndpoints();

builder.AddProject<Projects.RssService>("rssservice")
    .WithReference(cosmosConnectionString)
    .WaitFor(cosmosConnectionString)
    .WithReference(hangfireMongoConnectionString)
    .WaitFor(hangfireMongoConnectionString)
    .WithHttpEndpoint(port: rssPort)
    .WithExternalHttpEndpoints();

builder.AddProject<Projects.ApiService>("apiservice")
    .WithReference(cosmosConnectionString)
    .WaitFor(cosmosConnectionString)
    .WithReference(hangfireMongoConnectionString)
    .WaitFor(hangfireMongoConnectionString)
    .WithHttpEndpoint(port: apiPort)
    .WithExternalHttpEndpoints();

var app = builder.Build();

// Opens WebApp's own endpoints once it's had time to finish starting - the Aspire dashboard
// itself already auto-opens via launchSettings.json's launchBrowser. RssService/ApiService have
// no real pages to open (just a health-check listener), so only WebApp is worth launching a
// browser tab for. Best-effort only: a missing display/browser (e.g. headless CI) is swallowed,
// never fails the AppHost itself.
_ = Task.Run(async () =>
{
    await Task.Delay(TimeSpan.FromSeconds(8));
    TryOpenBrowser($"http://localhost:{webAppPort}/scalar/v1");
});

app.Run();

static void TryOpenBrowser(string url)
{
    try
    {
        if (OperatingSystem.IsMacOS())
        {
            System.Diagnostics.Process.Start("open", url);
        }
        else if (OperatingSystem.IsWindows())
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true });
        }
        else
        {
            System.Diagnostics.Process.Start("xdg-open", url);
        }
    }
    catch
    {
        // No display/browser available (e.g. headless CI) - not fatal to the AppHost.
    }
}
