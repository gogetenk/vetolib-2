using FluentAssertions;
using Vetolib.MedicalRecords.Application.Domain;
using Xunit;

namespace Vetolib.Tests.Unit.MedicalRecords;

public class SharedRecordLinkDomainTests
{
    private static readonly Guid ValidClinicId = Guid.NewGuid();
    private static readonly Guid ValidPatientId = Guid.NewGuid();
    private static readonly Guid ValidOwnerAccountId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidData_ReturnsSuccess()
    {
        var result = SharedRecordLink.Create(ValidClinicId, ValidPatientId, ValidOwnerAccountId);

        result.IsSuccess.Should().BeTrue();
        result.Value.ClinicId.Should().Be(ValidClinicId);
        result.Value.PatientId.Should().Be(ValidPatientId);
        result.Value.OwnerAccountId.Should().Be(ValidOwnerAccountId);
        result.Value.Token.Should().NotBeNullOrWhiteSpace();
        result.Value.Token.Length.Should().BeGreaterOrEqualTo(40); // 32 bytes base64url ~ 43 chars
        result.Value.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddHours(72), TimeSpan.FromMinutes(1));
        result.Value.AccessCount.Should().Be(0);
        result.Value.RevokedAt.Should().BeNull();
    }

    [Fact]
    public void Create_WithEmptyClinicId_ReturnsInvalid()
    {
        var result = SharedRecordLink.Create(Guid.Empty, ValidPatientId, ValidOwnerAccountId);

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "clinicId");
    }

    [Fact]
    public void Create_WithEmptyPatientId_ReturnsInvalid()
    {
        var result = SharedRecordLink.Create(ValidClinicId, Guid.Empty, ValidOwnerAccountId);

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "patientId");
    }

    [Fact]
    public void Create_WithEmptyOwnerAccountId_ReturnsInvalid()
    {
        var result = SharedRecordLink.Create(ValidClinicId, ValidPatientId, Guid.Empty);

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "ownerAccountId");
    }

    [Fact]
    public void Create_WithAllEmptyIds_ReturnsMultipleErrors()
    {
        var result = SharedRecordLink.Create(Guid.Empty, Guid.Empty, Guid.Empty);

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().HaveCount(3);
    }

    [Fact]
    public void Create_GeneratesUrlSafeToken()
    {
        var result = SharedRecordLink.Create(ValidClinicId, ValidPatientId, ValidOwnerAccountId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Token.Should().NotContain("+");
        result.Value.Token.Should().NotContain("/");
        result.Value.Token.Should().NotContain("=");
    }

    [Fact]
    public void Create_GeneratesUniqueTokens()
    {
        var result1 = SharedRecordLink.Create(ValidClinicId, ValidPatientId, ValidOwnerAccountId);
        var result2 = SharedRecordLink.Create(ValidClinicId, ValidPatientId, ValidOwnerAccountId);

        result1.Value.Token.Should().NotBe(result2.Value.Token);
    }

    [Fact]
    public void IsActive_WhenNewLink_ReturnsTrue()
    {
        var link = SharedRecordLink.Create(ValidClinicId, ValidPatientId, ValidOwnerAccountId).Value;

        link.IsActive().Should().BeTrue();
        link.IsExpired().Should().BeFalse();
        link.IsRevoked().Should().BeFalse();
    }

    [Fact]
    public void Revoke_WhenActive_SetsRevokedAt()
    {
        var link = SharedRecordLink.Create(ValidClinicId, ValidPatientId, ValidOwnerAccountId).Value;

        var result = link.Revoke();

        result.IsSuccess.Should().BeTrue();
        link.RevokedAt.Should().NotBeNull();
        link.IsRevoked().Should().BeTrue();
        link.IsActive().Should().BeFalse();
    }

    [Fact]
    public void Revoke_WhenAlreadyRevoked_ReturnsError()
    {
        var link = SharedRecordLink.Create(ValidClinicId, ValidPatientId, ValidOwnerAccountId).Value;
        link.Revoke();

        var result = link.Revoke();

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void IncrementAccessCount_WhenActive_IncrementsCount()
    {
        var link = SharedRecordLink.Create(ValidClinicId, ValidPatientId, ValidOwnerAccountId).Value;

        var result = link.IncrementAccessCount();

        result.IsSuccess.Should().BeTrue();
        link.AccessCount.Should().Be(1);
    }

    [Fact]
    public void IncrementAccessCount_MultipleTimes_AccumulatesCorrectly()
    {
        var link = SharedRecordLink.Create(ValidClinicId, ValidPatientId, ValidOwnerAccountId).Value;

        link.IncrementAccessCount();
        link.IncrementAccessCount();
        link.IncrementAccessCount();

        link.AccessCount.Should().Be(3);
    }

    [Fact]
    public void IncrementAccessCount_WhenRevoked_ReturnsError()
    {
        var link = SharedRecordLink.Create(ValidClinicId, ValidPatientId, ValidOwnerAccountId).Value;
        link.Revoke();

        var result = link.IncrementAccessCount();

        result.IsSuccess.Should().BeFalse();
    }
}
