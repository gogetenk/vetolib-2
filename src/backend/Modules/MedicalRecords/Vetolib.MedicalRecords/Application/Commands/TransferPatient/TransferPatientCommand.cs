using Ardalis.Result;
using MediatR;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.MedicalRecords.Application.Commands.TransferPatient;

internal record TransferPatientCommand(
    Guid SourceClinicId,
    Guid PatientId,
    Guid TargetClinicId,
    bool IncludeRecords,
    bool IncludeWeightHistory,
    string TransferredBy) : IRequest<Result<TransferPatientResultDto>>;
