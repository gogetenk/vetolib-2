using Ardalis.Result;
using Vetolib.Preferences.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.Preferences.Application.Domain;

internal class ClinicPreferenceDefault : BaseEntity, IMultiTenant
{
    public Guid ClinicId { get; private set; }
    public PreferenceCategory Category { get; private set; }
    public PreferenceKey Key { get; private set; }
    public string Value { get; private set; } = string.Empty;

    private ClinicPreferenceDefault() { } // EF Core

    public static Result<ClinicPreferenceDefault> Create(
        Guid clinicId,
        PreferenceKey key,
        string value)
    {
        var errors = new List<ValidationError>();

        if (clinicId == Guid.Empty)
            errors.Add(new ValidationError(nameof(clinicId), "ClinicId is required"));

        if (string.IsNullOrWhiteSpace(value))
            errors.Add(new ValidationError(nameof(value), "Value is required"));

        if (key == PreferenceKey.AIDrugInteractions &&
            value.Equals("false", StringComparison.OrdinalIgnoreCase))
        {
            errors.Add(new ValidationError(nameof(key),
                "AIDrugInteractions cannot be disabled. This feature is always active for patient safety."));
        }

        if (errors.Count > 0)
            return Result<ClinicPreferenceDefault>.Invalid(errors);

        return Result<ClinicPreferenceDefault>.Success(new ClinicPreferenceDefault
        {
            ClinicId = clinicId,
            Category = SystemDefaults.GetCategory(key),
            Key = key,
            Value = value
        });
    }

    public Result Update(string newValue)
    {
        if (string.IsNullOrWhiteSpace(newValue))
            return Result.Invalid(new ValidationError(nameof(newValue), "Value is required"));

        if (Key == PreferenceKey.AIDrugInteractions &&
            newValue.Equals("false", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Invalid(new ValidationError(nameof(newValue),
                "AIDrugInteractions cannot be disabled. This feature is always active for patient safety."));
        }

        Value = newValue;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public PreferenceDto ToDto() => new(Category, Key, Value, PreferenceSource.Clinic);
}
