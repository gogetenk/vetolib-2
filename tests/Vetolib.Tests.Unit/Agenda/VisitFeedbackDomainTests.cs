using Ardalis.Result;
using FluentAssertions;
using Vetolib.Agenda.Application.Domain;
using Xunit;

namespace Vetolib.Tests.Unit.Agenda;

public class VisitFeedbackDomainTests
{
    private static readonly Guid ClinicId = new("11111111-1111-1111-1111-111111111111");
    private static readonly Guid AppointmentId = new("22222222-2222-2222-2222-222222222222");

    [Fact]
    public void Create_WithValidData_ReturnsSuccess()
    {
        var result = VisitFeedback.Create(ClinicId, AppointmentId, 5, "Great service!", true);

        result.IsSuccess.Should().BeTrue();
        result.Value.ClinicId.Should().Be(ClinicId);
        result.Value.AppointmentId.Should().Be(AppointmentId);
        result.Value.Rating.Should().Be(5);
        result.Value.Comment.Should().Be("Great service!");
        result.Value.IsPublic.Should().BeTrue();
    }

    [Fact]
    public void Create_WithNullComment_ReturnsSuccess()
    {
        var result = VisitFeedback.Create(ClinicId, AppointmentId, 3, null, false);

        result.IsSuccess.Should().BeTrue();
        result.Value.Comment.Should().BeNull();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(6)]
    [InlineData(100)]
    public void Create_WithInvalidRating_ReturnsInvalid(int rating)
    {
        var result = VisitFeedback.Create(ClinicId, AppointmentId, rating, null, false);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
        result.ValidationErrors.Should().Contain(e => e.Identifier == "rating");
    }

    [Fact]
    public void Create_WithEmptyClinicId_ReturnsInvalid()
    {
        var result = VisitFeedback.Create(Guid.Empty, AppointmentId, 4, null, false);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
        result.ValidationErrors.Should().Contain(e => e.Identifier == "clinicId");
    }

    [Fact]
    public void Create_WithEmptyAppointmentId_ReturnsInvalid()
    {
        var result = VisitFeedback.Create(ClinicId, Guid.Empty, 4, null, false);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
        result.ValidationErrors.Should().Contain(e => e.Identifier == "appointmentId");
    }

    [Fact]
    public void Create_WithCommentExceeding1000Chars_ReturnsInvalid()
    {
        var longComment = new string('x', 1001);

        var result = VisitFeedback.Create(ClinicId, AppointmentId, 4, longComment, false);

        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Invalid);
        result.ValidationErrors.Should().Contain(e => e.Identifier == "comment");
    }

    [Fact]
    public void Create_WithCommentExactly1000Chars_ReturnsSuccess()
    {
        var exactComment = new string('x', 1000);

        var result = VisitFeedback.Create(ClinicId, AppointmentId, 4, exactComment, false);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void ToDto_MapsAllProperties()
    {
        var feedback = VisitFeedback.Create(ClinicId, AppointmentId, 5, "Excellent!", true).Value;

        var dto = feedback.ToDto();

        dto.Id.Should().Be(feedback.Id);
        dto.AppointmentId.Should().Be(AppointmentId);
        dto.Rating.Should().Be(5);
        dto.Comment.Should().Be("Excellent!");
        dto.IsPublic.Should().BeTrue();
        dto.CreatedAt.Should().Be(feedback.CreatedAt);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void Create_WithValidRatingRange_ReturnsSuccess(int rating)
    {
        var result = VisitFeedback.Create(ClinicId, AppointmentId, rating, null, false);

        result.IsSuccess.Should().BeTrue();
        result.Value.Rating.Should().Be(rating);
    }
}
