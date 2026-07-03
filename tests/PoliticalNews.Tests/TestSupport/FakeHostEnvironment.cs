using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace PoliticalNews.Tests.TestSupport;

/// <summary>Minimal <see cref="IHostEnvironment"/> double for orchestrator tests - only EnvironmentName/ApplicationName are ever read.</summary>
public sealed class FakeHostEnvironment : IHostEnvironment
{
    public string EnvironmentName { get; set; } = "Test";

    public string ApplicationName { get; set; } = "PoliticalNews.Tests";

    public string ContentRootPath { get; set; } = AppContext.BaseDirectory;

    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
}
