using Ardalis.Result;
using FluentAssertions;
using NSubstitute;
using Vetolib.Agenda.Application.Queries.ListClinicVeterinarians;
using Vetolib.Agenda.Contracts;
using Vetolib.Auth.Contracts;
using Xunit;

namespace Vetolib.Tests.Unit.Agenda;

public class ListClinicVeterinariansHandlerTests
{
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");

    private readonly IClinicVetReader _vetReader;
    private readonly ListClinicVeterinariansHandler _handler;

    public ListClinicVeterinariansHandlerTests()
    {
        _vetReader = Substitute.For<IClinicVetReader>();
        _handler = new ListClinicVeterinariansHandler(_vetReader);
    }

    [Fact]
    public async Task Handle_WhenVetsExist_ReturnsVetList()
    {
        // Arrange
        var vets = new List<ClinicVetDto>
        {
            new(new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "Dr. Ahmed Al-Rashidi"),
            new(new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), "Dr. Fatima Al-Zahrawi")
        };

        _vetReader.GetVeterinariansForClinic(ClinicId, Arg.Any<CancellationToken>())
            .Returns(Result<List<ClinicVetDto>>.Success(vets));

        var query = new ListClinicVeterinariansQuery(ClinicId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().Contain(v => v.Name == "Dr. Ahmed Al-Rashidi");
        result.Value.Should().Contain(v => v.Name == "Dr. Fatima Al-Zahrawi");
    }

    [Fact]
    public async Task Handle_WhenNoVets_ReturnsEmptyList()
    {
        // Arrange
        _vetReader.GetVeterinariansForClinic(ClinicId, Arg.Any<CancellationToken>())
            .Returns(Result<List<ClinicVetDto>>.Success(new List<ClinicVetDto>()));

        var query = new ListClinicVeterinariansQuery(ClinicId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_MapsClinicVetDtoToClinicVeterinarianDto()
    {
        // Arrange
        var vetId = new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc");
        var vets = new List<ClinicVetDto>
        {
            new(vetId, "Dr. Omar Al-Mansoori")
        };

        _vetReader.GetVeterinariansForClinic(ClinicId, Arg.Any<CancellationToken>())
            .Returns(Result<List<ClinicVetDto>>.Success(vets));

        var query = new ListClinicVeterinariansQuery(ClinicId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var vet = result.Value.Single();
        vet.Id.Should().Be(vetId);
        vet.Name.Should().Be("Dr. Omar Al-Mansoori");
    }
}
