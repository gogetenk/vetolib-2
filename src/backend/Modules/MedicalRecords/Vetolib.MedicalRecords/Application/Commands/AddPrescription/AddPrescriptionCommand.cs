using Ardalis.Result;
using MediatR;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.MedicalRecords.Application.Commands.AddPrescription;

internal record AddPrescriptionCommand(
    Guid ClinicId,
    Guid MedicalRecordId,
    Guid PatientId,
    string Medication,
    string Dosage,
    string VetLicenseNumber,
    Guid VetId,
    string UserRole,
    Guid? DrugCatalogEntryId = null,
    decimal? DosageAmount = null,
    string? OverrideJustification = null) : IRequest<Result<PrescriptionDto>>;
