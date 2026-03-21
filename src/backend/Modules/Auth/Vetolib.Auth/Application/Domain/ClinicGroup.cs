using Ardalis.Result;
using Vetolib.Shared.Kernel;

namespace Vetolib.Auth.Application.Domain;

/// <summary>
/// Represents a group of clinics owned by the same user.
/// Cross-tenant entity: NOT IMultiTenant — it spans multiple clinics by design.
/// </summary>
internal class ClinicGroup : BaseEntity, IAggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public Guid OwnerUserId { get; private set; }

    private readonly List<ClinicGroupMember> _members = [];
    public IReadOnlyCollection<ClinicGroupMember> Members => _members.AsReadOnly();

    private ClinicGroup() { } // EF Core constructor

    public static Result<ClinicGroup> Create(string name, Guid ownerUserId)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(name))
            errors.Add(new ValidationError(nameof(name), "Group name is required"));

        if (ownerUserId == Guid.Empty)
            errors.Add(new ValidationError(nameof(ownerUserId), "Owner user ID is required"));

        if (errors.Count > 0)
            return Result<ClinicGroup>.Invalid(errors);

        var group = new ClinicGroup
        {
            Name = name.Trim(),
            OwnerUserId = ownerUserId
        };

        return Result<ClinicGroup>.Success(group);
    }

    public Result AddClinic(Guid clinicId)
    {
        if (clinicId == Guid.Empty)
            return Result.Invalid(new ValidationError(nameof(clinicId), "Clinic ID is required"));

        if (_members.Any(m => m.ClinicId == clinicId))
            return Result.Conflict("Clinic is already a member of this group");

        _members.Add(ClinicGroupMember.Create(Id, clinicId));
        return Result.Success();
    }

    public Result RemoveClinic(Guid clinicId)
    {
        var member = _members.FirstOrDefault(m => m.ClinicId == clinicId);
        if (member is null)
            return Result.NotFound("Clinic is not a member of this group");

        _members.Remove(member);
        return Result.Success();
    }
}
