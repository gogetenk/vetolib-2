using Ardalis.Result;
using MediatR;

namespace Vetolib.MedicalRecords.Application.Queries.ExportPatientFhir;

internal record ExportPatientFhirQuery(Guid PatientId) : IRequest<Result<FhirBundleResult>>;

internal record FhirBundleResult(string Json, string ContentType)
{
    public const string FhirJsonContentType = "application/fhir+json";
}
