using Ardalis.Result;
using Vetolib.Agenda.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.Agenda.Application.Domain;

internal class VisitFeedback : BaseEntity, IMultiTenant
{
    public Guid ClinicId { get; private set; }
    public Guid AppointmentId { get; private set; }
    public int Rating { get; private set; }
    public string? Comment { get; private set; }
    public bool IsPublic { get; private set; }

    private VisitFeedback() { } // EF Core constructor

    public static Result<VisitFeedback> Create(
        Guid clinicId,
        Guid appointmentId,
        int rating,
        string? comment,
        bool isPublic)
    {
        var errors = new List<ValidationError>();

        if (clinicId == Guid.Empty)
            errors.Add(new ValidationError(nameof(clinicId), "ClinicId is required"));

        if (appointmentId == Guid.Empty)
            errors.Add(new ValidationError(nameof(appointmentId), "AppointmentId is required"));

        if (rating < 1 || rating > 5)
            errors.Add(new ValidationError(nameof(rating), "Rating must be between 1 and 5"));

        if (comment is not null && comment.Length > 1000)
            errors.Add(new ValidationError(nameof(comment), "Comment must not exceed 1000 characters"));

        if (errors.Count > 0)
            return Result<VisitFeedback>.Invalid(errors);

        var feedback = new VisitFeedback
        {
            ClinicId = clinicId,
            AppointmentId = appointmentId,
            Rating = rating,
            Comment = comment,
            IsPublic = isPublic
        };

        return Result<VisitFeedback>.Success(feedback);
    }

    public VisitFeedbackDto ToDto()
    {
        return new VisitFeedbackDto(
            Id,
            AppointmentId,
            Rating,
            Comment,
            IsPublic,
            CreatedAt);
    }
}
