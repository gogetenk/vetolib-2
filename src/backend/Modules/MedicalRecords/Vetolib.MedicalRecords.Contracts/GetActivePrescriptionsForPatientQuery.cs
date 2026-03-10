using Ardalis.Result;
using MediatR;

namespace Vetolib.MedicalRecords.Contracts;

public record GetActivePrescriptionsForPatientQuery(
    Guid PatientId,
    int ActiveWindowDays = 90) : IRequest<Result<List<PrescriptionDto>>>;
