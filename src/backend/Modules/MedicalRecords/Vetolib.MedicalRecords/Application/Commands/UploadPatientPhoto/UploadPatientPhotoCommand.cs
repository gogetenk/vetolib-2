using Ardalis.Result;
using MediatR;

namespace Vetolib.MedicalRecords.Application.Commands.UploadPatientPhoto;

internal record UploadPatientPhotoCommand(
    Guid PatientId,
    byte[] PhotoData,
    string ContentType) : IRequest<Result>;
