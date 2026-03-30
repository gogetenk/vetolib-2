using Ardalis.Result;
using MediatR;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.MedicalRecords.Application.Queries.ListMedicalRecordTemplates;

internal record ListMedicalRecordTemplatesQuery(
    TemplateCategory? Category,
    Species? Species) : IRequest<Result<List<MedicalRecordTemplateDto>>>;
