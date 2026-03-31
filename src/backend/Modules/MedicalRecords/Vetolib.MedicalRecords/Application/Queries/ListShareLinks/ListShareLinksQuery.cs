using Ardalis.Result;
using MediatR;
using Vetolib.MedicalRecords.Contracts;

namespace Vetolib.MedicalRecords.Application.Queries.ListShareLinks;

internal record ListShareLinksQuery(Guid OwnerAccountId) : IRequest<Result<IReadOnlyList<SharedRecordLinkDto>>>;
