using FluentAssertions;
using Vetolib.MedicalRecords.Application.Commands.TransferPatient;
using Vetolib.MedicalRecords.Application.Domain;
using Vetolib.MedicalRecords.Contracts;
using Xunit;

namespace Vetolib.Tests.Unit.MedicalRecords;

public class TransferPatientTests
{
    private static readonly Guid SourceClinicId = Guid.NewGuid();
    private static readonly Guid TargetClinicId = Guid.NewGuid();

    // ── Patient.MarkAsTransferred domain tests ───────────────────────

    [Fact]
    public void MarkAsTransferred_WithValidTargetClinic_ReturnsSuccess()
    {
        var patient = Patient.Create(SourceClinicId, "Rocky", Species.Dog, "Labrador", new DateOnly(2021, 5, 10)).Value;

        var result = patient.MarkAsTransferred(TargetClinicId);

        result.IsSuccess.Should().BeTrue();
        patient.TransferredToClinicId.Should().Be(TargetClinicId);
        patient.TransferredAt.Should().NotBeNull();
        patient.IsTransferred.Should().BeTrue();
    }

    [Fact]
    public void MarkAsTransferred_WithEmptyGuid_ReturnsError()
    {
        var patient = Patient.Create(SourceClinicId, "Rocky", Species.Dog, "Labrador", new DateOnly(2021, 5, 10)).Value;

        var result = patient.MarkAsTransferred(Guid.Empty);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("required"));
    }

    [Fact]
    public void MarkAsTransferred_ToSameClinic_ReturnsError()
    {
        var patient = Patient.Create(SourceClinicId, "Rocky", Species.Dog, "Labrador", new DateOnly(2021, 5, 10)).Value;

        var result = patient.MarkAsTransferred(SourceClinicId);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("same clinic"));
    }

    [Fact]
    public void MarkAsTransferred_AlreadyTransferred_ReturnsError()
    {
        var patient = Patient.Create(SourceClinicId, "Rocky", Species.Dog, "Labrador", new DateOnly(2021, 5, 10)).Value;
        patient.MarkAsTransferred(TargetClinicId);

        var result = patient.MarkAsTransferred(Guid.NewGuid());

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("already been transferred"));
    }

    [Fact]
    public void Patient_InitialState_IsNotTransferred()
    {
        var patient = Patient.Create(SourceClinicId, "Rocky", Species.Dog, "Labrador", new DateOnly(2021, 5, 10)).Value;

        patient.IsTransferred.Should().BeFalse();
        patient.TransferredToClinicId.Should().BeNull();
        patient.TransferredAt.Should().BeNull();
    }

    // ── TransferLog domain tests ─────────────────────────────────────

    [Fact]
    public void TransferLog_Create_WithValidData_ReturnsSuccess()
    {
        var result = TransferLog.Create(SourceClinicId, TargetClinicId, Guid.NewGuid(), "Dr. Ahmed");

        result.IsSuccess.Should().BeTrue();
        result.Value.SourceClinicId.Should().Be(SourceClinicId);
        result.Value.TargetClinicId.Should().Be(TargetClinicId);
        result.Value.Status.Should().Be(TransferStatus.Pending);
        result.Value.ClinicId.Should().Be(SourceClinicId);
    }

    [Fact]
    public void TransferLog_Create_WithEmptySource_ReturnsInvalid()
    {
        var result = TransferLog.Create(Guid.Empty, TargetClinicId, Guid.NewGuid(), "Dr. Ahmed");

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "sourceClinicId");
    }

    [Fact]
    public void TransferLog_Create_WithEmptyTarget_ReturnsInvalid()
    {
        var result = TransferLog.Create(SourceClinicId, Guid.Empty, Guid.NewGuid(), "Dr. Ahmed");

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "targetClinicId");
    }

    [Fact]
    public void TransferLog_Create_WithSameSourceAndTarget_ReturnsInvalid()
    {
        var result = TransferLog.Create(SourceClinicId, SourceClinicId, Guid.NewGuid(), "Dr. Ahmed");

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "targetClinicId");
    }

    [Fact]
    public void TransferLog_Create_WithEmptyPatientId_ReturnsInvalid()
    {
        var result = TransferLog.Create(SourceClinicId, TargetClinicId, Guid.Empty, "Dr. Ahmed");

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "patientId");
    }

    [Fact]
    public void TransferLog_Create_WithEmptyTransferredBy_ReturnsInvalid()
    {
        var result = TransferLog.Create(SourceClinicId, TargetClinicId, Guid.NewGuid(), "");

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "transferredBy");
    }

    [Fact]
    public void TransferLog_MarkCompleted_SetsStatusToCompleted()
    {
        var log = TransferLog.Create(SourceClinicId, TargetClinicId, Guid.NewGuid(), "Dr. Ahmed").Value;

        log.MarkCompleted();

        log.Status.Should().Be(TransferStatus.Completed);
    }

    [Fact]
    public void TransferLog_MarkFailed_SetsStatusAndReason()
    {
        var log = TransferLog.Create(SourceClinicId, TargetClinicId, Guid.NewGuid(), "Dr. Ahmed").Value;

        log.MarkFailed("Something went wrong");

        log.Status.Should().Be(TransferStatus.Failed);
        log.FailureReason.Should().Be("Something went wrong");
    }

    // ── TransferPatientValidator tests ───────────────────────────────

    [Fact]
    public void Validator_ValidCommand_Passes()
    {
        var validator = new TransferPatientValidator();
        var cmd = new TransferPatientCommand(SourceClinicId, Guid.NewGuid(), TargetClinicId, true, true, "Dr. Ahmed");

        var result = validator.Validate(cmd);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_EmptySourceClinicId_Fails()
    {
        var validator = new TransferPatientValidator();
        var cmd = new TransferPatientCommand(Guid.Empty, Guid.NewGuid(), TargetClinicId, true, true, "Dr. Ahmed");

        var result = validator.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "SourceClinicId");
    }

    [Fact]
    public void Validator_EmptyPatientId_Fails()
    {
        var validator = new TransferPatientValidator();
        var cmd = new TransferPatientCommand(SourceClinicId, Guid.Empty, TargetClinicId, true, true, "Dr. Ahmed");

        var result = validator.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "PatientId");
    }

    [Fact]
    public void Validator_EmptyTargetClinicId_Fails()
    {
        var validator = new TransferPatientValidator();
        var cmd = new TransferPatientCommand(SourceClinicId, Guid.NewGuid(), Guid.Empty, true, true, "Dr. Ahmed");

        var result = validator.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "TargetClinicId");
    }

    [Fact]
    public void Validator_SameSourceAndTargetClinic_Fails()
    {
        var validator = new TransferPatientValidator();
        var cmd = new TransferPatientCommand(SourceClinicId, Guid.NewGuid(), SourceClinicId, true, true, "Dr. Ahmed");

        var result = validator.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("same clinic"));
    }

    [Fact]
    public void Validator_EmptyTransferredBy_Fails()
    {
        var validator = new TransferPatientValidator();
        var cmd = new TransferPatientCommand(SourceClinicId, Guid.NewGuid(), TargetClinicId, true, true, "");

        var result = validator.Validate(cmd);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "TransferredBy");
    }
}
