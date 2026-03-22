namespace Vetolib.Auth.Application;

internal class AuthSecurityOptions
{
    public const string SectionName = "Auth:Security";

    /// <summary>
    /// Maximum failed login attempts before account lockout (default: 5).
    /// </summary>
    public int MaxFailedLoginAttempts { get; set; } = 5;

    /// <summary>
    /// Account lockout duration in minutes (default: 15).
    /// </summary>
    public int LockoutMinutes { get; set; } = 15;

    /// <summary>
    /// JWT access token expiration in minutes (default: 15).
    /// </summary>
    public int TokenExpirationMinutes { get; set; } = 15;

    /// <summary>
    /// Trial period in days for new clinics (default: 14).
    /// </summary>
    public int TrialDays { get; set; } = 14;
}
