using Ardalis.Result;
using Vetolib.Shared.Kernel;

namespace Vetolib.MedicalRecords.Application.Domain;

internal enum TransferStatus
{
    Pending,
    Completed,
    Failed
}

internal class TransferLog : BaseEntity, IMultiTenant
{
    public Guid ClinicId { get; private set; }
    public Guid SourceClinicId { get; private set; }
    public Guid TargetClinicId { get; private set; }
    public Guid PatientId { get; private set; }
    public DateTime TransferredAt { get; private set; }
    public string TransferredBy { get; private set; } = string.Empty;
    public TransferStatus Status { get; private set; }
    public string? FailureReason { get; private set; }

    private TransferLog() { } // EF Core constructor

    public static Result<TransferLog> Create(Guid sourceClinicId, Guid targetClinicId, Guid patientId, string transferredBy)
    {
        var errors = new List<ValidationError>();

        if (sourceClinicId == Guid.Empty)
            errors.Add(new ValidationError(nameof(sourceClinicId), "Source clinic ID is required"));

        if (targetClinicId == Guid.Empty)
            errors.Add(new ValidationError(nameof(targetClinicId), "Target clinic ID is required"));

        if (patientId == Guid.Empty)
            errors.Add(new ValidationError(nameof(patientId), "Patient ID is required"));

        if (string.IsNullOrWhiteSpace(transferredBy))
            errors.Add(new ValidationError(nameof(transferredBy), "TransferredBy is required"));

        if (sourceClinicId == targetClinicId)
            errors.Add(new ValidationError(nameof(targetClinicId), "Cannot transfer to the same clinic"));

        if (errors.Count > 0)
            return Result<TransferLog>.Invalid(errors);

        return Result<TransferLog>.Success(new TransferLog
        {
            ClinicId = sourceClinicId, // belongs to source clinic for query filter
            SourceClinicId = sourceClinicId,
            TargetClinicId = targetClinicId,
            PatientId = patientId,
            TransferredAt = DateTime.UtcNow,
            TransferredBy = transferredBy,
            Status = TransferStatus.Pending
        });
    }

    public void MarkCompleted()
    {
        Status = TransferStatus.Completed;
    }

    public void MarkFailed(string reason)
    {
        Status = TransferStatus.Failed;
        FailureReason = reason;
    }
}
