using Ardalis.Result;
using MediatR;
using Vetolib.Preferences.Contracts;

namespace Vetolib.Preferences.Application.Queries.GetConsentAudit;

internal record GetConsentAuditQuery(
    Guid? UserId = null,
    PreferenceCategory? Category = null,
    DateTime? From = null,
    DateTime? To = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<Result<ConsentAuditPagedResultDto>>;
