using Ardalis.Result;
using MediatR;

namespace Vetolib.MedicalRecords.Application.Queries.GetPatientPhoto;

internal record GetPatientPhotoQuery(Guid PatientId) : IRequest<Result<PatientPhotoResult>>;

internal record PatientPhotoResult(byte[] Data, string ContentType);
