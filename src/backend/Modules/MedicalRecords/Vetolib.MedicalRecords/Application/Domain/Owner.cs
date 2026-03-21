using Ardalis.Result;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.MedicalRecords.Application.Domain;

internal class Owner : BaseEntity, IMultiTenant
{
    public Guid ClinicId { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string? Phone { get; private set; }

    private readonly List<PatientOwner> _patientOwners = [];
    public IReadOnlyList<PatientOwner> PatientOwners => _patientOwners.AsReadOnly();

    private Owner() { } // EF Core constructor

    public static Result<Owner> Create(Guid clinicId, string firstName, string lastName, string email, string? phone)
    {
        var errors = new List<ValidationError>();

        if (clinicId == Guid.Empty)
            errors.Add(new ValidationError(nameof(clinicId), "ClinicId is required"));

        if (string.IsNullOrWhiteSpace(firstName))
            errors.Add(new ValidationError(nameof(firstName), "First name is required"));

        if (string.IsNullOrWhiteSpace(lastName))
            errors.Add(new ValidationError(nameof(lastName), "Last name is required"));

        if (string.IsNullOrWhiteSpace(email))
            errors.Add(new ValidationError(nameof(email), "Email is required"));

        if (errors.Count > 0)
            return Result<Owner>.Invalid(errors);

        var owner = new Owner
        {
            ClinicId = clinicId,
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            Phone = phone?.Trim()
        };

        return Result<Owner>.Success(owner);
    }

    public void UpdatePhone(string phone)
    {
        Phone = phone.Trim();
    }

    public void UpdateName(string fullName)
    {
        var parts = fullName.Trim().Split(' ', 2);
        FirstName = parts[0];
        LastName = parts.Length > 1 ? parts[1] : LastName;
    }

    public OwnerDto ToDto()
    {
        return new OwnerDto(Id, FirstName, LastName, Email, Phone, ClinicId);
    }
}
