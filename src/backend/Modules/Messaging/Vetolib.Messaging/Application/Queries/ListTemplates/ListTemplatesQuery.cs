using Ardalis.Result;
using MediatR;
using Vetolib.Messaging.Contracts;

namespace Vetolib.Messaging.Application.Queries.ListTemplates;

internal record ListTemplatesQuery(string? Category = null)
    : IRequest<Result<IReadOnlyList<ResponseTemplateDto>>>;
