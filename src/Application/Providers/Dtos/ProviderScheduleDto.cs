namespace Application.Providers.Dtos;

/// <summary>Read projection of one provider's live, database-backed schedule - returned after an edit from the Provider Management page.</summary>
public sealed record ProviderScheduleDto(string Pipeline, string Provider, bool Enabled, string Cron, string TimeZone);
