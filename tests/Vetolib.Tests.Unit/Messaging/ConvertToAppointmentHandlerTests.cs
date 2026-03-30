using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.Messaging.Application.Commands.ConvertToAppointment;
using Vetolib.Messaging.Application.Domain;
using Vetolib.Messaging.Contracts;
using Vetolib.Messaging.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Messaging;

public class ConvertToAppointmentHandlerTests : IDisposable
{
    private static readonly Guid ClinicId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid OwnerId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid PatientId = Guid.Parse("44444444-4444-4444-4444-444444444444");

    private readonly MessagingDbContext _context;
    private readonly IPatientReader _patientReader;
    private readonly ConvertToAppointmentHandler _sut;

    public ConvertToAppointmentHandlerTests()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        var options = new DbContextOptionsBuilder<MessagingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new MessagingDbContext(options, clinicContext, Substitute.For<IPublisher>());
        _patientReader = Substitute.For<IPatientReader>();
        _sut = new ConvertToAppointmentHandler(_context, _patientReader, clinicContext);
    }

    [Fact]
    public async Task Handle_WhenConversationHasPatient_ReturnsAppointmentRequest()
    {
        var conversation = CreateConversationWithPatient();
        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync();

        // Add an owner message so there's context
        var message = Message.Create(conversation.Id, MessageSender.Owner, null, "My dog is limping").Value;
        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        SetupPatientReader("Rex", "Ahmed Al-Rashid");

        var cmd = new ConvertToAppointmentCommand(conversation.Id, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)), null);

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.PatientId.Should().Be(PatientId);
        result.Value.OwnerId.Should().Be(OwnerId);
        result.Value.AnimalName.Should().Be("Rex");
        result.Value.OwnerName.Should().Be("Ahmed Al-Rashid");
    }

    [Fact]
    public async Task Handle_WhenConversationNotFound_ReturnsNotFound()
    {
        var cmd = new ConvertToAppointmentCommand(Guid.NewGuid(), null, null);

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_WhenNoPatientLinked_ReturnsError()
    {
        var conversation = Conversation.Create(
            ClinicId, OwnerId, null, "No patient", MessageCategory.Administrative).Value;
        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync();

        var cmd = new ConvertToAppointmentCommand(conversation.Id, null, null);

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("MISSING_PATIENT"));
    }

    [Fact]
    public async Task Handle_WithExplicitNotes_UsesProvidedNotes()
    {
        var conversation = CreateConversationWithPatient();
        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync();

        SetupPatientReader("Buddy", "Sara Khan");

        var cmd = new ConvertToAppointmentCommand(conversation.Id, null, "Needs vaccination");

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Notes.Should().Be("Needs vaccination");
    }

    [Fact]
    public async Task Handle_WhenPatientReaderFails_UsesUnknownNames()
    {
        var conversation = CreateConversationWithPatient();
        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync();

        _patientReader.GetPatientsByOwnerIdAsync(OwnerId, ClinicId, Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyList<PatientDto>>.Error("Service unavailable"));

        var cmd = new ConvertToAppointmentCommand(conversation.Id, null, "Notes");

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AnimalName.Should().Be("Unknown");
        result.Value.OwnerName.Should().Be("Unknown");
    }

    private Conversation CreateConversationWithPatient()
    {
        return Conversation.Create(
            ClinicId,
            OwnerId,
            PatientId,
            "Test conversation",
            MessageCategory.MedicalQuestion).Value;
    }

    private void SetupPatientReader(string animalName, string ownerName)
    {
        var patients = new List<PatientDto>
        {
            new(PatientId, animalName, Species.Dog, "Labrador", DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-3)),
                Sex.Male, ownerName, "+971501234567", ClinicId)
        };
        _patientReader.GetPatientsByOwnerIdAsync(OwnerId, ClinicId, Arg.Any<CancellationToken>())
            .Returns(Result<IReadOnlyList<PatientDto>>.Success(patients));
    }

    public void Dispose() => _context.Dispose();
}
