using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.MedicalRecords.Application.Domain;
using Vetolib.MedicalRecords.Application.Queries.ListPatients;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.MedicalRecords.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.MedicalRecords;

public class ListPatientsHandlerTests : IDisposable
{
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");

    private readonly MedicalRecordsDbContext _context;
    private readonly ListPatientsHandler _handler;

    public ListPatientsHandlerTests()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        var publisher = Substitute.For<IPublisher>();

        var options = new DbContextOptionsBuilder<MedicalRecordsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new MedicalRecordsDbContext(options, clinicContext, publisher);
        _handler = new ListPatientsHandler(_context);
    }

    private Patient SeedPatient(string name, Species species, string breed, string? microchip = null)
    {
        var result = Patient.Create(ClinicId, name, species, breed, new DateOnly(2020, 5, 10), microchipNumber: microchip);
        var patient = result.Value;
        _context.Patients.Add(patient);
        _context.SaveChanges();
        return patient;
    }

    private Owner SeedOwner(string firstName, string lastName, string email, string? phone = null)
    {
        var result = Owner.Create(ClinicId, firstName, lastName, email, phone);
        var owner = result.Value;
        _context.Owners.Add(owner);
        _context.SaveChanges();
        return owner;
    }

    private void LinkOwnerToPatient(Patient patient, Owner owner)
    {
        var po = PatientOwner.Create(ClinicId, patient.Id, owner.Id);
        _context.PatientOwners.Add(po);
        _context.SaveChanges();
    }

    [Fact]
    public async Task Handle_NoFilters_ReturnsAllPatients()
    {
        SeedPatient("Bella", Species.Dog, "Labrador");
        SeedPatient("Luna", Species.Cat, "Siamese");

        var result = await _handler.Handle(new ListPatientsQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Total.Should().Be(2);
        result.Value.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_FilterByName_CaseInsensitivePartialMatch()
    {
        SeedPatient("Bella", Species.Dog, "Labrador");
        SeedPatient("Luna", Species.Cat, "Siamese");
        SeedPatient("Bellissima", Species.Dog, "Poodle");

        var result = await _handler.Handle(new ListPatientsQuery(Name: "bell"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Total.Should().Be(2);
        result.Value.Items.Should().AllSatisfy(p => p.Name.Should().ContainEquivalentOf("bell"));
    }

    [Fact]
    public async Task Handle_FilterByName_UpperCaseQuery_StillMatches()
    {
        SeedPatient("bella", Species.Dog, "Labrador");

        var result = await _handler.Handle(new ListPatientsQuery(Name: "BELLA"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Total.Should().Be(1);
    }

    [Fact]
    public async Task Handle_FilterBySpecies_ReturnsOnlyMatchingSpecies()
    {
        SeedPatient("Bella", Species.Dog, "Labrador");
        SeedPatient("Luna", Species.Cat, "Siamese");
        SeedPatient("Rex", Species.Dog, "Shepherd");

        var result = await _handler.Handle(new ListPatientsQuery(Species: Species.Cat), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Total.Should().Be(1);
        result.Value.Items[0].Name.Should().Be("Luna");
    }

    [Fact]
    public async Task Handle_FilterByMicrochip_ExactMatch()
    {
        SeedPatient("Bella", Species.Dog, "Labrador", microchip: "123456789012345");
        SeedPatient("Luna", Species.Cat, "Siamese", microchip: "987654321098765");

        var result = await _handler.Handle(new ListPatientsQuery(Microchip: "123456789012345"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Total.Should().Be(1);
        result.Value.Items[0].Name.Should().Be("Bella");
    }

    [Fact]
    public async Task Handle_FilterByOwnerPhone_PartialMatch()
    {
        var bella = SeedPatient("Bella", Species.Dog, "Labrador");
        var luna = SeedPatient("Luna", Species.Cat, "Siamese");

        var owner1 = SeedOwner("Ahmed", "Al-Rashid", "ahmed@email.ae", "+971501234567");
        var owner2 = SeedOwner("Fatima", "Hassan", "fatima@email.ae", "+971559876543");

        LinkOwnerToPatient(bella, owner1);
        LinkOwnerToPatient(luna, owner2);

        var result = await _handler.Handle(new ListPatientsQuery(OwnerPhone: "501234"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Total.Should().Be(1);
        result.Value.Items[0].Name.Should().Be("Bella");
    }

    [Fact]
    public async Task Handle_FilterByOwnerPhone_NoMatch_ReturnsEmpty()
    {
        var bella = SeedPatient("Bella", Species.Dog, "Labrador");
        var owner = SeedOwner("Ahmed", "Al-Rashid", "ahmed@email.ae", "+971501234567");
        LinkOwnerToPatient(bella, owner);

        var result = await _handler.Handle(new ListPatientsQuery(OwnerPhone: "999999999"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Total.Should().Be(0);
    }

    [Fact]
    public async Task Handle_CombinedFilters_NameAndSpecies()
    {
        SeedPatient("Bella", Species.Dog, "Labrador");
        SeedPatient("Bella", Species.Cat, "Persian");
        SeedPatient("Luna", Species.Dog, "Poodle");

        var result = await _handler.Handle(new ListPatientsQuery(Name: "Bella", Species: Species.Dog), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Total.Should().Be(1);
        result.Value.Items[0].Species.Should().Be(Species.Dog);
    }

    [Fact]
    public async Task Handle_CombinedFilters_SpeciesAndOwnerPhone()
    {
        var bella = SeedPatient("Bella", Species.Dog, "Labrador");
        var luna = SeedPatient("Luna", Species.Dog, "Poodle");

        var owner1 = SeedOwner("Ahmed", "Al-Rashid", "ahmed@email.ae", "+971501234567");
        var owner2 = SeedOwner("Fatima", "Hassan", "fatima@email.ae", "+971559876543");

        LinkOwnerToPatient(bella, owner1);
        LinkOwnerToPatient(luna, owner2);

        var result = await _handler.Handle(new ListPatientsQuery(Species: Species.Dog, OwnerPhone: "501234"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Total.Should().Be(1);
        result.Value.Items[0].Name.Should().Be("Bella");
    }

    [Fact]
    public async Task Handle_Pagination_ReturnsCorrectPage()
    {
        for (int i = 0; i < 5; i++)
            SeedPatient($"Patient_{i:D2}", Species.Dog, "Labrador");

        var result = await _handler.Handle(new ListPatientsQuery(Page: 2, PageSize: 2), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Total.Should().Be(5);
        result.Value.Items.Should().HaveCount(2);
        result.Value.Page.Should().Be(2);
        result.Value.PageSize.Should().Be(2);
    }

    [Fact]
    public async Task Handle_InvalidPageSize_DefaultsTo20()
    {
        SeedPatient("Bella", Species.Dog, "Labrador");

        var result = await _handler.Handle(new ListPatientsQuery(PageSize: -1), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.PageSize.Should().Be(20);
    }

    [Fact]
    public async Task Handle_PageSizeOver100_DefaultsTo20()
    {
        SeedPatient("Bella", Species.Dog, "Labrador");

        var result = await _handler.Handle(new ListPatientsQuery(PageSize: 200), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.PageSize.Should().Be(20);
    }

    [Fact]
    public async Task Handle_EmptyDatabase_ReturnsEmptyResult()
    {
        var result = await _handler.Handle(new ListPatientsQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Total.Should().Be(0);
        result.Value.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_AllFiltersCombined()
    {
        var bella = SeedPatient("Bella", Species.Dog, "Labrador", microchip: "123456789012345");
        var owner = SeedOwner("Ahmed", "Al-Rashid", "ahmed@email.ae", "+971501234567");
        LinkOwnerToPatient(bella, owner);

        SeedPatient("Luna", Species.Cat, "Siamese");

        var result = await _handler.Handle(
            new ListPatientsQuery(
                Name: "bell",
                Species: Species.Dog,
                Microchip: "123456789012345",
                OwnerPhone: "501234"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Total.Should().Be(1);
        result.Value.Items[0].Name.Should().Be("Bella");
    }

    public void Dispose() => _context.Dispose();
}
