using Ardalis.Result;
using MediatR;

namespace Vetolib.MedicalRecords.Application.Commands.RevokeShareLink;

internal record RevokeShareLinkCommand(
    Guid LinkId,
    Guid OwnerAccountId) : IRequest<Result>;
