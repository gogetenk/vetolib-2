namespace Vetolib.Auth.Contracts;

public record ClinicSearchResultDto(
    Guid Id,
    string Name,
    string? City,
    IReadOnlyList<string> SupportedSpecies,
    string? LogoUrl,
    string? Slug);
