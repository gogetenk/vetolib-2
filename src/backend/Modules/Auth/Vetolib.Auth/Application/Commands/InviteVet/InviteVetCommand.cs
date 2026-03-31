using Ardalis.Result;
using MediatR;

namespace Vetolib.Auth.Application.Commands.InviteVet;

internal record InviteVetCommand(
    string VetEmail,
    string OwnerName,
    string PetName,
    string? Message) : IRequest<Result>;
