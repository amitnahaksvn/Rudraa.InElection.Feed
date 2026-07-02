# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Build everything
dotnet build Rudraa.InElection.RSSFeed.slnx      # note: .slnx, not .sln

# Run all tests
dotnet test tests/PoliticalNews.Tests/PoliticalNews.Tests.csproj

# Run a single test
dotnet test tests/PoliticalNews.Tests/PoliticalNews.Tests.csproj --filter "FullyQualifiedName~NewsCrawlerOrchestratorTests.RunCrawlAsync_FeedFailure_RecordsCompletedWithErrorsAndFailedFeedName"

# Run the crawler (scheduled background service, no HTTP surface)
dotnet run --project src/PoliticalNews.Worker

# Run the read/query API standalone (Minimal API endpoints dispatch through Mediator)
dotnet run --project src/PoliticalNews.Web

# Run everything together via the Aspire AppHost (local Mongo container + Web + Worker,
# with the Aspire dashboard for logs/traces/metrics) - requires Docker running
dotnet run --project src/PoliticalNews.AppHost

# Or without Aspire/Docker at all
docker compose up --build
```

Real credentials (e.g. an Atlas connection string) belong in user-secrets, never in
`appsettings.json`:
```bash
dotnet user-secrets set "MongoDb:ConnectionString" "mongodb+srv://..." --project src/PoliticalNews.Worker
dotnet user-secrets set "MongoDb:DatabaseName" "SomeDbName" --project src/PoliticalNews.Worker
# repeat --project src/PoliticalNews.Web if running Web standalone (outside the AppHost)
# for the AppHost itself: dotnet user-secrets set "ConnectionStrings:mongodb" "mongodb+srv://..." --project src/PoliticalNews.AppHost
#                          dotnet user-secrets set "UseLocalMongo" "false" --project src/PoliticalNews.AppHost
```
MongoDB database names cannot contain `.` (or `/ \ " $ * < > : | ?` / spaces) - this rejects a
name copy-pasted straight from a domain-style string like `Foo.Bar`.

## Architecture

Clean Architecture, dependency direction is strictly inward:
`Domain <- Application <- Infrastructure <- (Web | Worker)`. `Domain` has zero dependencies
(not even on MongoDB.Driver - entities are plain POCOs); Mongo BSON class maps are registered
centrally in `Infrastructure/Mongo/MongoClassMapConfigurator.cs`, not via attributes on entities.

**Two composition roots, one crawl engine.** `PoliticalNews.Worker` (a plain `BackgroundService`
console host, no HTTP) runs the cron schedule; `PoliticalNews.Web` exposes read endpoints plus a
manual trigger. Both ultimately call the same `INewsCrawlerService.RunCrawlAsync` implementation
(`NewsCrawlerOrchestrator` in `Application/Services`), which internally acquires a Mongo-backed
distributed lock (`ICrawlLockRepository`) before running - so a scheduled tick and a manually
triggered API call can never execute concurrently.

**Application layer is CQRS via `Mediator` (source-generator based, not MediatR - swapped
deliberately to avoid MediatR's commercial licensing) + FluentValidation.** Each query/command
lives in its own file under `Application/News/Queries/*`, `Application/Crawl/Queries/*`, or
`Application/Crawl/Commands/*`, containing both the request record and its `IRequestHandler` in
that same file, plus a sibling `*Validator.cs`. Pipeline behaviours (`Application/Common/Behaviours`)
run in this order: Logging -> UnhandledException -> Validation -> Performance. Handlers return
`ValueTask<TResponse>` (not `Task`) - that's the `Mediator` package's API, distinct from MediatR.

**`Web` is Minimal API, not MVC controllers - deliberately, so nothing should be named
`*Controller`.** `Web/Endpoints/{Feature}.cs` (`News.cs`, `Crawl.cs`) are static classes
implementing the marker interface `Web/Infrastructure/IEndpointGroup`, each with a
`public static void Map(RouteGroupBuilder groupBuilder)` and `public static` handler methods
(`ISender sender, ...` as parameters, returning `Task<Ok<T>>`/`Task<Results<...>>` via
`TypedResults`). `Web/Infrastructure/WebApplicationExtensions.MapEndpoints(Assembly)` reflection-scans
for `IEndpointGroup` implementations and invokes their static `Map` method - adding a new feature's
endpoints means adding one class, nothing to register by hand in `Program.cs`.
`Web/Infrastructure/EndpointRouteBuilderExtensions` adds `MapGet`/`MapPost`/etc. overloads typed to
`RouteGroupBuilder` specifically (so they're picked over the ASP.NET Core built-ins by C#'s
more-specific-receiver rule) that auto-derive `WithName(handler.Method.Name)`; `Program.cs`'s
`CustomOperationIds` then surfaces that same name as the OpenAPI `operationId`. DTOs returned by
handlers (`Application/News/Dtos`, `Application/Crawl/Dtos`) are also the HTTP response shape -
there is no separate Web-layer contract/mapping type. Errors (including FluentValidation failures
*and* Minimal API's own parameter-binding failures, i.e. `BadHttpRequestException`) are centralized
in `Web/Infrastructure/ProblemDetailsExceptionHandler` -> RFC7807 ProblemDetails; endpoints never
try/catch.

**RSS provider abstraction is designed for multi-provider expansion.** `IRssProvider`
(`Application/Abstractions`) is implemented by `BaseRssProvider` (`Infrastructure/RssProviders`),
which owns the entire generic pipeline: HTTP fetch, RSS 2.0 XML parsing, image extraction
(`media:content` -> `media:thumbnail` -> `enclosure` -> `og:image` HTML fallback), and
normalization into `NormalizedArticle`. A concrete provider (`AajTakRssProvider`, `AbpNewsRssProvider`)
supplies just `Name` and an `IHttpClientFactory` client name - no MongoDB or persistence code.
Adding a provider for a later phase (ANI/NDTV/PIB/...) means: one new `BaseRssProvider` subclass,
one `services.AddHttpClient(...)` + `AddSingleton<IRssProvider, ...>()` in
`Infrastructure/DependencyInjection/InfrastructureServiceCollectionExtensions.cs`, and one new
block under `NewsCrawler:Providers` in `appsettings.json`. Feed URLs are never hardcoded.

**Each provider owns its own cron schedule**, not a single global one: `RssProviderOptions.Cron`
(a standard 5-field expression) lives per provider block. Scheduling is Hangfire, not a hand-rolled
timer loop: `Worker/Program.cs` registers one native Hangfire recurring job per enabled provider at
startup, job id `news-crawl-{provider}` (`Infrastructure/Scheduling/HangfireJobIds.NewsCrawl`) -
`AddOrUpdate` is idempotent, so every restart just re-syncs each job's cron expression against
config rather than creating duplicates. `Hangfire.Mongo` backs job storage (collections prefixed
`hangfire`, separate from the app's own collections); only `Worker` calls `AddHangfireServer()` and
actually executes jobs - `Web` registers the same Mongo storage read-only (no server) purely so it
can host the dashboard (`app.UseHangfireDashboard()` at `/hangfire`, gated behind
`Api:EnableHangfireDashboard` - off by default because Hangfire's dashboard has no built-in auth; on
in `appsettings.Development.json`). `MongoStorageOptions.CheckConnection = false` on both - Hangfire.Mongo's
default synchronous startup ping crashes the whole host if Atlas is briefly slow to answer, which
Mongo connectivity is already verified elsewhere anyway. A manually triggered crawl
(`POST /api/crawl/trigger`) still runs every enabled provider regardless of its individual schedule
(`RunCrawlAsync(CancellationToken)`, no provider filter) - independent of Hangfire, straight through
`NewsCrawlerOrchestrator`.

**Every Hangfire job invokes `Infrastructure/Scheduling/HangfireCrawlJobExecutor.RunAsync`, never
`INewsCrawlerService` directly** - both from `Worker/Program.cs`'s startup registration and from
`HangfireCrawlJobTrigger.CreateOrUpdate` (below). This one indirection buys two things: a
`[JobDisplayName("Crawl {0}")]` so the Hangfire dashboard shows "Crawl AajTak" instead of a raw
method signature, and a `ILogger.BeginScope` tagging every log line the run produces with that
specific Hangfire job execution's own id (via the auto-injected `PerformContext`, never
serialized/caller-supplied) - so a run seen in the dashboard can be traced back to its exact logs.
It's also the one place enforcing that a Hangfire job can only ever do this one fixed, safe thing
(crawl a specific already-configured provider) - never arbitrary caller-supplied code.

**Recurring-job management beyond static config, all through `ICrawlJobTrigger`/`ICrawlJobStatusReader`
(`Application/Abstractions`, implemented in `Infrastructure/Scheduling`) so Application never
references Hangfire types directly:**
- `POST /api/crawl/trigger/{provider}` - enqueues that provider's job to run now, ahead of its cron,
  without changing the schedule; returns immediately (the crawl runs asynchronously wherever
  `Worker`'s Hangfire server is), unlike `POST /api/crawl/trigger`.
- `POST /api/crawl/jobs` (body: `jobName`, `cron`, `timeZone` - defaults to `"UTC"`) - creates or
  updates one provider's recurring job live. `jobName` must already be an enabled provider with a
  `Cron` in `NewsCrawler.appsettings.json` (validated via `CreateOrUpdateRecurringJobCommandValidator`,
  which also syntax-checks `cron` via `Cronos.CronExpression.Parse` and `timeZone` via
  `TimeZoneInfo.FindSystemTimeZoneById` - both 400 on failure, not a 500). This is a **live
  override only** - it does not persist to `NewsCrawler.appsettings.json`, so it lasts until
  `Worker` next restarts and re-registers every provider's job from that file.
- `GET /api/crawl/jobs/{provider}` - current schedule plus the last run's Hangfire job id/state
  (`Succeeded`/`Failed`/`Processing`/...) and, if it failed, the exception type/message - 404 if no
  recurring job is registered for that provider.
- `GET /api/crawl/history/{id}` - single `CrawlHistory` record by its Mongo id, 404 if not found.

**`NewsCrawler:*` config lives in one shared file, not duplicated per project:**
`src/NewsCrawler.appsettings.json` (providers/feeds/schedules) is linked into both
`PoliticalNews.Worker.csproj` and `PoliticalNews.Web.csproj` via a `Content Include="..\NewsCrawler.appsettings.json"`
item and loaded in both `Program.cs` files via `AddJsonFile(Path.Combine(AppContext.BaseDirectory, ...))`
- `AppContext.BaseDirectory`, not `ContentRootPath`, because `dotnet run` sets the latter to the
project's source directory while a published/Docker deployment's is its own output folder, and
`AppContext.BaseDirectory` is the one location consistent between the two. `Web` needs this too
because its manual trigger runs a real crawl in-process, not just reads. The source is inserted
*before* the environment-variables source in the config chain (not appended, which is
`CreateBuilder`'s default for anything added after it) specifically so `NewsCrawler__*` env vars -
e.g. `NewsCrawler__Providers__0__Cron`, `NewsCrawler__SaveRawResponses` - can still override the
shared file, matching the same env-var-wins convention already used for `MongoDb:ConnectionString`.

**Duplicate detection order** (`Infrastructure/Persistence/NewsArticleRepository.UpsertAsync`):
`Url` -> `OriginalGuid` -> `Hash` (SHA-256 of normalized title + `PublishedAt`, computed by
`Application/Services/ArticleHasher`). A match on `Url`/`OriginalGuid` with changed content
(title/summary/content/image) updates the existing document; a match with no change, or any
`Hash` match, is a no-op duplicate skip. Articles are never duplicated.

**Configuration resolution for Mongo is dual-path by design:** `Infrastructure`'s
`AddInfrastructure()` binds `MongoDb:ConnectionString` from `appsettings.json`/user-secrets, but
`PostConfigure` overrides it with `ConnectionStrings:mongodb` when present. That second key is
what the Aspire AppHost (`PoliticalNews.AppHost/AppHost.cs`) injects automatically via
`WithReference(...)`, so the exact same `Infrastructure`/`Web`/`Worker` code runs unchanged
whether launched through the AppHost, plain `dotnet run`, or docker-compose. `AddInfrastructure()`
also registers `MongoIndexInitializerHostedService` (in `Infrastructure/Mongo`, shared by both
hosts) - it runs on startup before either host starts serving, creating every collection/index
automatically the first time either process connects to a brand-new database.

**`PoliticalNews.ServiceDefaults`** (Aspire's shared-project convention) is referenced by both
`Web` and `Worker` and adds OpenTelemetry tracing/metrics/logging, default health checks, service
discovery, and HTTP resilience via one `builder.AddServiceDefaults()` call. `Web` additionally
calls `app.MapDefaultEndpoints()` to expose `/health` and `/alive` (Worker has no HTTP listener,
so it only gets the telemetry/resilience side). `AppHost/AppHost.cs`'s `UseLocalMongo` toggle
picks between an Aspire-managed local Mongo container (`Aspire.Hosting.MongoDB`, default, zero
credentials needed) and an external connection string resource (`AddConnectionString("mongodb")`,
e.g. a real Atlas cluster) - no code outside `AppHost.cs` needs to know which one is active.

## Known data caveats

Only one Aaj Tak (`aajtak.in`) RSS feed could be publicly verified (`?id=home` - no other
category-slug or numeric-id pattern resolved). The rest of the `NewsCrawler:Providers[Name=AajTak]`
feed list in `appsettings.json` are `tak.live` feeds (India Today Group's sister "Tak" video
network - `news-tak`, `crime-tak`, `bharat-tak`, etc.), grouped under the same `AajTak` provider
block per explicit instruction. Three tak.live slugs that were tried and don't resolve
(`sports-tak`, `mumbai-tak`, `short-videos`) are intentionally excluded, not omissions.

ABP Live (`abplive.com`) exposes a plain, consistent `{path}/feed` pattern (no `?id=` numeric
scheme like Aaj Tak) - every one of the 54 feeds under `NewsCrawler:Providers[Name=ABPNews]` was
individually curl-verified (HTTP 200, well-formed `<rss>` body) before being added, covering
national/world/state news, elections, fact-check, business, entertainment, lifestyle, astrology,
and a few utility categories (education, agriculture, GK, web-stories). `abplive.com` is the
Hindi-language edition (`Language: "hi"`, matching AajTak); ABP's other language editions
(`bengali.`, `marathi.`, `tamil.`, `telugu.`, `gujarati.`, `punjabi.`, `news.` for English) live on
separate subdomains and are not wired in.
