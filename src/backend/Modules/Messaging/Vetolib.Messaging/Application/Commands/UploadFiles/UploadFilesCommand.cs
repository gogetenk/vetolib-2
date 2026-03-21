using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Vetolib.Messaging.Application.Commands.UploadFiles;

internal record UploadFilesCommand(IFormFileCollection Files) : IRequest<Result<List<Guid>>>;
