using System.Reflection;
using PoliticalNews.Application.DependencyInjection;
using PoliticalNews.Infrastructure.DependencyInjection;
using PoliticalNews.Web.Infrastructure;
using PoliticalNews.Web.Options;
using Scalar.AspNetCore;

// `dotnet run --project src/PoliticalNews.Web -- --init-db` creates every MongoDB
// collection/index (see MongoIndexInitializerHostedService) and exits - a repeatable, idempotent
// database setup script with no Kestrel/HTTP surface started.
var initDbOnly = args.Contains("--init-db", StringComparer.OrdinalIgnoreCase);

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddApplication(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

if (initDbOnly)
{
    // Sole ServiceProvider built in this process for this mode (we `return` right after, the
    // normal `builder.Build()`/Kestrel path below never runs), so there's no risk of the usual
    // "two copies of singletons" problem ASP0000 warns about.
#pragma warning disable ASP0000
    using var initProvider = builder.Services.BuildServiceProvider();
#pragma warning restore ASP0000
    var initLogger = initProvider.GetRequiredService<ILogger<Program>>();

    initLogger.LogInformation("--init-db: creating MongoDB collections/indexes only");
    foreach (var hostedService in initProvider.GetServices<IHostedService>())
    {
        await hostedService.StartAsync(CancellationToken.None);
    }
    initLogger.LogInformation("--init-db: done");
    return;
}

builder.Services
    .AddOptions<ApiOptions>()
    .Bind(builder.Configuration.GetSection(ApiOptions.SectionName));

builder.Services.AddExceptionHandler<ProblemDetailsExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddHealthChecks().AddMongoDb(name: "mongodb");

var enableSwagger = builder.Configuration.GetValue($"{ApiOptions.SectionName}:EnableSwagger", true);
if (enableSwagger)
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new()
        {
            Title = "Political News Crawler API",
            Version = "v1",
            Description = "Read-only access to crawled news articles and crawl run history, plus a manual crawl trigger."
        });

        // Endpoint groups set WithName(handler.Method.Name) (see EndpointRouteBuilderExtensions);
        // surface that same name as the OpenAPI operationId for stable NSwag/typed-client generation.
        options.CustomOperationIds(apiDescription =>
            apiDescription.ActionDescriptor.EndpointMetadata.OfType<IEndpointNameMetadata>().FirstOrDefault()?.EndpointName);
    });
}

var app = builder.Build();

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseExceptionHandler();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

// Deliberately no UseHttpsRedirection(): under Aspire the HTTPS port is assigned dynamically per
// run (ASPNETCORE_HTTPS_PORT can go stale and redirect to the wrong port), and on a PaaS like
// Render.com, TLS is terminated at the platform's edge and this container only ever sees plain
// HTTP internally - redirecting there would either loop or send clients to the wrong place either way.

if (enableSwagger)
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Scalar reads the same Swashbuckle-generated document - no second OpenAPI generator needed.
    app.MapScalarApiReference(options =>
    {
        options.WithOpenApiRoutePattern("/swagger/{documentName}/swagger.json");
        options.Title = "Political News Crawler API";
    });

    app.MapGet("/", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();
}

app.MapDefaultEndpoints();
app.MapEndpoints(Assembly.GetExecutingAssembly());

app.Run();
