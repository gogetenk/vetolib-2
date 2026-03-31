using Ardalis.Result;
using MediatR;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.MedicalRecords.Application.Commands.ImportPatientFhir;

internal record ImportPatientFhirCommand(
    Guid ClinicId,
    string FhirBundleJson) : IRequest<Result<FhirImportResultDto>>;
