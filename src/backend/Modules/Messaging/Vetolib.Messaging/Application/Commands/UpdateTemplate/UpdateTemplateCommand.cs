using Ardalis.Result;
using MediatR;
using Vetolib.Messaging.Contracts;

namespace Vetolib.Messaging.Application.Commands.UpdateTemplate;

internal record UpdateTemplateCommand(
    Guid Id,
    string Name,
    string ContentEn,
    string ContentAr,
    MessageCategory? Category
) : IRequest<Result<ResponseTemplateDto>>;
