using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Vetolib.Messaging.Contracts;
using Vetolib.Messaging.Infrastructure;

namespace Vetolib.Messaging.Application.Commands.UpdateTemplate;

internal class UpdateTemplateHandler : IRequestHandler<UpdateTemplateCommand, Result<ResponseTemplateDto>>
{
    private readonly MessagingDbContext _context;

    public UpdateTemplateHandler(MessagingDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ResponseTemplateDto>> Handle(UpdateTemplateCommand command, CancellationToken ct)
    {
        var template = await _context.ResponseTemplates
            .FirstOrDefaultAsync(t => t.Id == command.Id, ct);

        if (template is null)
            return Result<ResponseTemplateDto>.NotFound($"Template {command.Id} not found.");

        var updateResult = template.Update(
            command.Name,
            command.ContentEn,
            command.ContentAr,
            command.Category);

        if (!updateResult.IsSuccess)
            return Result<ResponseTemplateDto>.Invalid(updateResult.ValidationErrors);

        await _context.SaveChangesAsync(ct);

        return Result<ResponseTemplateDto>.Success(template.ToDto());
    }
}
