using Ardalis.Result;
using FluentAssertions;
using Vetolib.MedicalRecords.Application.Domain;
using Xunit;

namespace Vetolib.Tests.Unit.MedicalRecords;

public class OwnerPortalDomainTests
{
    private static readonly Guid ClinicId = Guid.NewGuid();
    private static readonly Guid PatientId = Guid.NewGuid();

    [Fact]
    public void MedicalRecord_IsVisibleToOwner_DefaultsToTrue()
    {
        var result = MedicalRecord.Create(
            ClinicId, PatientId, "Checkup", "Healthy", "Dr. Ahmed", DateTime.UtcNow);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsVisibleToOwner.Should().BeTrue();
    }

    [Fact]
    public void MedicalRecord_SetVisibilityToOwner_False_HidesRecord()
    {
        var record = MedicalRecord.Create(
            ClinicId, PatientId, "Checkup", "Healthy", "Dr. Ahmed", DateTime.UtcNow).Value;

        var result = record.SetVisibilityToOwner(false);

        result.IsSuccess.Should().BeTrue();
        record.IsVisibleToOwner.Should().BeFalse();
    }

    [Fact]
    public void MedicalRecord_SetVisibilityToOwner_True_ShowsRecord()
    {
        var record = MedicalRecord.Create(
            ClinicId, PatientId, "Checkup", "Healthy", "Dr. Ahmed", DateTime.UtcNow).Value;

        record.SetVisibilityToOwner(false);
        var result = record.SetVisibilityToOwner(true);

        result.IsSuccess.Should().BeTrue();
        record.IsVisibleToOwner.Should().BeTrue();
    }

    [Fact]
    public void Owner_LinkOwnerAccount_WithValidId_Succeeds()
    {
        var owner = Owner.Create(ClinicId, "Ahmed", "Al-Rashid", "ahmed@email.ae", "+971 50 123 4567").Value;
        var accountId = Guid.NewGuid();

        var result = owner.LinkOwnerAccount(accountId);

        result.IsSuccess.Should().BeTrue();
        owner.OwnerAccountId.Should().Be(accountId);
    }

    [Fact]
    public void Owner_LinkOwnerAccount_WithEmptyId_ReturnsInvalid()
    {
        var owner = Owner.Create(ClinicId, "Ahmed", "Al-Rashid", "ahmed@email.ae", "+971 50 123 4567").Value;

        var result = owner.LinkOwnerAccount(Guid.Empty);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
    }

    [Fact]
    public void Owner_OwnerAccountId_DefaultsToNull()
    {
        var owner = Owner.Create(ClinicId, "Fatima", "Hassan", "fatima@email.ae", null).Value;

        owner.OwnerAccountId.Should().BeNull();
    }
}
