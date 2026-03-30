using Ardalis.Result;
using MediatR;

namespace Vetolib.Auth.Application.Commands.VerifyEmail;

internal record VerifyEmailCommand(string Token) : IRequest<Result>;
