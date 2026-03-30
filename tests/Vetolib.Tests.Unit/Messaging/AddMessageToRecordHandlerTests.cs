using Ardalis.Result;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Vetolib.MedicalRecords.Contracts;
using Vetolib.Messaging.Application.Commands.AddMessageToRecord;
using Vetolib.Messaging.Application.Domain;
using Vetolib.Messaging.Contracts;
using Vetolib.Messaging.Infrastructure;
using Vetolib.Shared.Kernel;
using Xunit;

namespace Vetolib.Tests.Unit.Messaging;

public class AddMessageToRecordHandlerTests : IDisposable
{
    private static readonly Guid ClinicId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid PatientId = Guid.Parse("44444444-4444-4444-4444-444444444444");

    private readonly MessagingDbContext _context;
    private readonly IPatientRecordWriter _recordWriter;
    private readonly AddMessageToRecordHandler _sut;

    public AddMessageToRecordHandlerTests()
    {
        var clinicContext = Substitute.For<IClinicContext>();
        clinicContext.ClinicId.Returns(ClinicId);

        var options = new DbContextOptionsBuilder<MessagingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new MessagingDbContext(options, clinicContext, Substitute.For<IPublisher>());
        _recordWriter = Substitute.For<IPatientRecordWriter>();
        _sut = new AddMessageToRecordHandler(_context, _recordWriter);
    }

    [Fact]
    public async Task Handle_WhenValidMessageAndPatient_CallsRecordWriterAndReturnsSuccess()
    {
        var conversation = CreateConversationWithPatient();
        var message = Message.Create(conversation.Id, MessageSender.Owner, null, "My cat has a rash").Value;
        _context.Conversations.Add(conversation);
        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        _recordWriter.AttachMessageNoteAsync(
            Arg.Any<AddMessageToRecordRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result<MedicalRecordNoteDto>.Success(
                new MedicalRecordNoteDto(Guid.NewGuid(), PatientId, "My cat has a rash", DateTime.UtcNow)));

        var cmd = new AddMessageToRecordCommand(conversation.Id, message.Id);

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _recordWriter.Received(1).AttachMessageNoteAsync(
            Arg.Is<AddMessageToRecordRequest>(r =>
                r.PatientId == PatientId &&
                r.MessageBody == "My cat has a rash"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenConversationNotFound_ReturnsNotFound()
    {
        var cmd = new AddMessageToRecordCommand(Guid.NewGuid(), Guid.NewGuid());

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_WhenConversationHasNoPatient_ReturnsError()
    {
        var conversation = CreateConversationWithoutPatient();
        var message = Message.Create(conversation.Id, MessageSender.Owner, null, "Test body").Value;
        _context.Conversations.Add(conversation);
        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        var cmd = new AddMessageToRecordCommand(conversation.Id, message.Id);

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("MISSING_PATIENT"));
    }

    [Fact]
    public async Task Handle_WhenMessageNotFoundInConversation_ReturnsNotFound()
    {
        var conversation = CreateConversationWithPatient();
        _context.Conversations.Add(conversation);
        await _context.SaveChangesAsync();

        var cmd = new AddMessageToRecordCommand(conversation.Id, Guid.NewGuid());

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.Status.Should().Be(ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_WhenMessageIsInternalNote_ReturnsError()
    {
        var conversation = CreateConversationWithPatient();
        var message = Message.Create(conversation.Id, MessageSender.Staff, null, "Internal note", isInternalNote: true).Value;
        _context.Conversations.Add(conversation);
        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        var cmd = new AddMessageToRecordCommand(conversation.Id, message.Id);

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("INTERNAL_NOTE_NOT_ALLOWED"));
    }

    [Fact]
    public async Task Handle_WhenRecordWriterFails_ReturnsError()
    {
        var conversation = CreateConversationWithPatient();
        var message = Message.Create(conversation.Id, MessageSender.Owner, null, "Test message").Value;
        _context.Conversations.Add(conversation);
        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        _recordWriter.AttachMessageNoteAsync(
            Arg.Any<AddMessageToRecordRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result<MedicalRecordNoteDto>.Error("Writer failed"));

        var cmd = new AddMessageToRecordCommand(conversation.Id, message.Id);

        var result = await _sut.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }

    private static Conversation CreateConversationWithPatient()
    {
        return Conversation.Create(
            ClinicId,
            Guid.NewGuid(),
            PatientId,
            "Test conversation",
            MessageCategory.MedicalQuestion).Value;
    }

    private static Conversation CreateConversationWithoutPatient()
    {
        return Conversation.Create(
            ClinicId,
            Guid.NewGuid(),
            null,
            "Test conversation",
            MessageCategory.Administrative).Value;
    }

    public void Dispose() => _context.Dispose();
}
