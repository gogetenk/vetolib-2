using Ardalis.Result;
using MediatR;

namespace Vetolib.MedicalRecords.Application.Commands.DeletePatientPhoto;

internal record DeletePatientPhotoCommand(Guid PatientId) : IRequest<Result>;
