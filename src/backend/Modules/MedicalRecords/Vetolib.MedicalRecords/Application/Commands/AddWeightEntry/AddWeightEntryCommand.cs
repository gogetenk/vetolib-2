using Ardalis.Result;
using MediatR;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.MedicalRecords.Application.Commands.AddWeightEntry;

internal record AddWeightEntryCommand(
    Guid ClinicId,
    Guid PatientId,
    decimal WeightKg,
    string RecordedBy,
    string? Note = null) : IRequest<Result<WeightEntryDto>>;
