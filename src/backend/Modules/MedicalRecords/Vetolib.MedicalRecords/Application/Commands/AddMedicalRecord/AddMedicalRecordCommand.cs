using Ardalis.Result;
using MediatR;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.MedicalRecords.Application.Commands.AddMedicalRecord;

internal record AddMedicalRecordCommand(
    Guid ClinicId,
    Guid PatientId,
    string Diagnosis,
    string Treatment,
    string VetName) : IRequest<Result<MedicalRecordDto>>;
