namespace PoliticalNews.Web.Options;

/// <summary>Root configuration section ("Api") controlling read-endpoint defaults and limits.</summary>
public sealed class ApiOptions
{
    public const string SectionName = "Api";

    public int DefaultPageSize { get; set; } = 20;

    public int MaxPageSize { get; set; } = 100;

    public bool EnableSwagger { get; set; } = true;
}
