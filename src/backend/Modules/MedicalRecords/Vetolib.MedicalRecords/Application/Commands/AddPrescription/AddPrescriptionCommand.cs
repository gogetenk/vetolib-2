using Ardalis.Result;
using MediatR;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.MedicalRecords.Application.Commands.AddPrescription;

internal record AddPrescriptionCommand(
    Guid ClinicId,
    Guid MedicalRecordId,
    string Medication,
    string Dosage,
    string VetLicenseNumber) : IRequest<Result<PrescriptionDto>>;
