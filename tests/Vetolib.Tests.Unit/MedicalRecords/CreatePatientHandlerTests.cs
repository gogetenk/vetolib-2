using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.MedicalRecords.Application.Commands.CreatePatient;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.MedicalRecords.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.MedicalRecords;

public class CreatePatientHandlerTests : IDisposable
{
    // Fixed GUID — EF Core bakes ClinicId into compiled queries via Expression.Constant
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");

    private readonly MedicalRecordsDbContext _context;
    private readonly CreatePatientHandler _handler;

    public CreatePatientHandlerTests()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        var publisher = Substitute.For<IPublisher>();

        var options = new DbContextOptionsBuilder<MedicalRecordsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new MedicalRecordsDbContext(options, clinicContext, publisher);
        _handler = new CreatePatientHandler(_context);
    }

    private CreatePatientCommand BuildCommand(
        string name = "Bella",
        Species species = Species.Dog,
        string breed = "Labrador",
        string ownerName = "Faisal Al-Kuwari",
        string ownerPhone = "+971501234567",
        DateOnly? birthDate = null,
        string? microchipNumber = null)
        => new(
            ClinicId: ClinicId,
            Name: name,
            Species: species,
            Breed: breed,
            BirthDate: birthDate ?? new DateOnly(2020, 5, 10),
            OwnerName: ownerName,
            OwnerPhone: ownerPhone,
            MicrochipNumber: microchipNumber);

    [Fact]
    public async Task Handle_HappyPath_CreatesPatientWithOwnerLink()
    {
        var cmd = BuildCommand();

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Bella");
        result.Value.Species.Should().Be(Species.Dog);
        result.Value.OwnerName.Should().Contain("Faisal");
        result.Value.OwnerPhone.Should().Be("+971501234567");
    }

    [Fact]
    public async Task Handle_HappyPath_PersistsPatientToDatabase()
    {
        var cmd = BuildCommand(name: "Zara");

        await _handler.Handle(cmd, CancellationToken.None);

        var count = await _context.Patients.CountAsync();
        count.Should().Be(1);
        var saved = await _context.Patients.FirstAsync();
        saved.Name.Should().Be("Zara");
    }

    [Fact]
    public async Task Handle_HappyPath_ReusesExistingOwnerByPhone()
    {
        // First patient creates the owner
        var cmd1 = BuildCommand(name: "Bella", ownerPhone: "+971509999999");
        await _handler.Handle(cmd1, CancellationToken.None);

        // Second patient with same phone should reuse the owner
        var cmd2 = BuildCommand(name: "Max", ownerPhone: "+971509999999");
        await _handler.Handle(cmd2, CancellationToken.None);

        var ownerCount = await _context.Owners.CountAsync();
        var patientCount = await _context.Patients.CountAsync();
        ownerCount.Should().Be(1);
        patientCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_WhenNameIsEmpty_ReturnsInvalid()
    {
        var cmd = BuildCommand(name: "");

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
    }

    [Fact]
    public async Task Handle_WhenNameIsWhitespace_ReturnsInvalid()
    {
        var cmd = BuildCommand(name: "   ");

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
    }

    [Fact]
    public async Task Handle_WhenBreedIsEmpty_ReturnsInvalid()
    {
        var cmd = BuildCommand(breed: "");

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
    }

    [Fact]
    public async Task Handle_WithMicrochip_CreatesPatientWithMicrochip()
    {
        var cmd = BuildCommand(microchipNumber: "900118000123456");

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.MicrochipNumber.Should().Be("900118000123456");
    }

    [Fact]
    public async Task Handle_WithoutMicrochip_CreatesPatientWithoutMicrochip()
    {
        var cmd = BuildCommand();

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.MicrochipNumber.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithDuplicateMicrochip_ReturnsConflict()
    {
        var cmd1 = BuildCommand(name: "Buddy", microchipNumber: "900118000111111", ownerPhone: "+971501111111");
        await _handler.Handle(cmd1, CancellationToken.None);

        var cmd2 = BuildCommand(name: "Max", microchipNumber: "900118000111111", ownerPhone: "+971502222222");
        var result = await _handler.Handle(cmd2, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Conflict);
    }

    public void Dispose() => _context.Dispose();
}
