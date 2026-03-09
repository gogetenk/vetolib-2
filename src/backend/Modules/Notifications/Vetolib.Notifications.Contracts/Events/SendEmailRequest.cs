namespace Vetolib.Notifications.Contracts.Events;

/// <summary>
/// Generic request to send an email using a named template.
/// Any module can publish this via MassTransit IBus.
/// </summary>
public record SendEmailRequest
{
    public string To { get; init; } = string.Empty;
    public string TemplateKey { get; init; } = string.Empty;
    public string Language { get; init; } = "en";
    /// <summary>
    /// Template variables serialised as a dictionary (key → value).
    /// </summary>
    public Dictionary<string, string> Data { get; init; } = new();
}
