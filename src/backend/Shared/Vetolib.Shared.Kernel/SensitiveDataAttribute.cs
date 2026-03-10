namespace Vetolib.Shared.Kernel;

/// <summary>
/// Marks a property as containing sensitive data that must be redacted in logs.
/// Used by the Serilog redaction pipeline to automatically mask values.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class SensitiveDataAttribute : Attribute;
