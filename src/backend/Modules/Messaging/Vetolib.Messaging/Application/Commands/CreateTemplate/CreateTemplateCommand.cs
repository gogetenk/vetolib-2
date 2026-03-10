using Ardalis.Result;
using MediatR;
using Vetolib.Messaging.Contracts;

namespace Vetolib.Messaging.Application.Commands.CreateTemplate;

internal record CreateTemplateCommand(
    Guid ClinicId,
    string Name,
    string ContentEn,
    string ContentAr,
    string? Category
) : IRequest<Result<ResponseTemplateDto>>;
