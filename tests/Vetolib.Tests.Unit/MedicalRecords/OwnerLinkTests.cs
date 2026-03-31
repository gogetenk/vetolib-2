using Ardalis.Result;
using FluentAssertions;
using Vetolib.MedicalRecords.Application.Domain;
using Xunit;

namespace Vetolib.Tests.Unit.MedicalRecords;

public class OwnerLinkTests
{
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid OwnerAccountId = new("22222222-2222-2222-2222-222222222222");

    [Fact]
    public void LinkToOwnerAccount_ValidId_SetsOwnerAccountId()
    {
        var owner = Owner.Create(ClinicId, "Fatima", "Al Rashid", "fatima@example.com", "+971501234567").Value;

        var result = owner.LinkToOwnerAccount(OwnerAccountId);

        result.IsSuccess.Should().BeTrue();
        owner.OwnerAccountId.Should().Be(OwnerAccountId);
    }

    [Fact]
    public void LinkToOwnerAccount_EmptyId_ReturnsInvalid()
    {
        var owner = Owner.Create(ClinicId, "Fatima", "Al Rashid", "fatima@example.com", "+971501234567").Value;

        var result = owner.LinkToOwnerAccount(Guid.Empty);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
    }
}
