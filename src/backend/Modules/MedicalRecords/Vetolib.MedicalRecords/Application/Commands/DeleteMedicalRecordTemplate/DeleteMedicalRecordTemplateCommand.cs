using Ardalis.Result;
using MediatR;

namespace Vetolib.MedicalRecords.Application.Commands.DeleteMedicalRecordTemplate;

internal record DeleteMedicalRecordTemplateCommand(Guid TemplateId) : IRequest<Result>;
