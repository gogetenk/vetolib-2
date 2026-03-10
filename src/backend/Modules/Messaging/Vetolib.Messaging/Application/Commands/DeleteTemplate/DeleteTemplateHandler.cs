using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Messaging.Infrastructure;

namespace Vetolib.Messaging.Application.Commands.DeleteTemplate;

internal class DeleteTemplateHandler : IRequestHandler<DeleteTemplateCommand, Result>
{
    private readonly MessagingDbContext _context;

    public DeleteTemplateHandler(MessagingDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteTemplateCommand command, CancellationToken ct)
    {
        var template = await _context.ResponseTemplates
            .FirstOrDefaultAsync(t => t.Id == command.Id, ct);

        if (template is null)
            return Result.NotFound($"Template {command.Id} not found.");

        _context.ResponseTemplates.Remove(template);
        await _context.SaveChangesAsync(ct);

        return Result.Success();
    }
}
