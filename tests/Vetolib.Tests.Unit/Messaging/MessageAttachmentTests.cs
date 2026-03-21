using FluentAssertions;
using Vetolib.Messaging.Application.Domain;
using Vetolib.Messaging.Contracts;
using Xunit;

namespace Vetolib.Tests.Unit.Messaging;

public class MessageAttachmentTests
{
    private static readonly Guid TestMessageId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    [Fact]
    public void Create_ValidPdf_ReturnsSuccess()
    {
        var result = MessageAttachment.Create(
            TestMessageId, "report.pdf", "application/pdf", 2048, "uploads/report.pdf");

        result.IsSuccess.Should().BeTrue();
        result.Value.ContentType.Should().Be("application/pdf");
    }

    [Fact]
    public void Create_ValidJpeg_ReturnsSuccess()
    {
        var result = MessageAttachment.Create(
            TestMessageId, "photo.jpg", "image/jpeg", 1024, "uploads/photo.jpg");

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_ValidPng_ReturnsSuccess()
    {
        var result = MessageAttachment.Create(
            TestMessageId, "xray.png", "image/png", 4096, "uploads/xray.png");

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_UnsupportedContentType_ReturnsInvalid()
    {
        var result = MessageAttachment.Create(
            TestMessageId, "doc.docx", "application/msword", 1024, "uploads/doc.docx");

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "contentType");
    }

    [Fact]
    public void Create_FileTooLarge_ReturnsInvalid()
    {
        var result = MessageAttachment.Create(
            TestMessageId, "huge.pdf", "application/pdf", 11 * 1024 * 1024, "uploads/huge.pdf");

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "fileSizeBytes");
    }

    [Fact]
    public void Create_ExactlyAtLimit_ReturnsSuccess()
    {
        var result = MessageAttachment.Create(
            TestMessageId, "big.pdf", "application/pdf", 10 * 1024 * 1024, "uploads/big.pdf");

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_EmptyMessageId_ReturnsInvalid()
    {
        var result = MessageAttachment.Create(
            Guid.Empty, "photo.jpg", "image/jpeg", 1024, "uploads/photo.jpg");

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Identifier == "messageId");
    }

    [Fact]
    public void ToDto_ReturnsCorrectValues()
    {
        var result = MessageAttachment.Create(
            TestMessageId, "photo.jpg", "image/jpeg", 1024, "uploads/photo.jpg");

        result.IsSuccess.Should().BeTrue();

        var dto = result.Value.ToDto("https://storage.example.com/photo.jpg?sig=abc");
        dto.Id.Should().Be(result.Value.Id);
        dto.MessageId.Should().Be(TestMessageId);
        dto.FileName.Should().Be("photo.jpg");
        dto.ContentType.Should().Be("image/jpeg");
        dto.FileSizeBytes.Should().Be(1024);
        dto.Url.Should().Be("https://storage.example.com/photo.jpg?sig=abc");
    }

    [Fact]
    public void AddAttachment_MaxAttachmentsReached_ReturnsError()
    {
        var messageResult = Message.Create(
            Guid.NewGuid(), MessageSender.Vet, Guid.NewGuid(), "Test message");
        messageResult.IsSuccess.Should().BeTrue();
        var message = messageResult.Value;

        // Add 3 attachments (the max)
        for (int i = 0; i < 3; i++)
        {
            var r = message.AddAttachment($"file{i}.pdf", "application/pdf", 1024, $"uploads/file{i}.pdf");
            r.IsSuccess.Should().BeTrue();
        }

        // 4th should fail
        var result = message.AddAttachment("file4.pdf", "application/pdf", 1024, "uploads/file4.pdf");
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("ATTACHMENT_LIMIT"));
    }
}
