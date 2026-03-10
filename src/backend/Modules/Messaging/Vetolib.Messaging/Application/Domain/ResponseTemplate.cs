using Ardalis.Result;
using Vetolib.Messaging.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.Messaging.Application.Domain;

internal class ResponseTemplate : BaseEntity, IMultiTenant
{
    public Guid ClinicId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string ContentEn { get; private set; } = string.Empty;
    public string ContentAr { get; private set; } = string.Empty;
    public string? Category { get; private set; }

    private ResponseTemplate() { } // EF Core

    public static Result<ResponseTemplate> Create(
        Guid clinicId,
        string name,
        string contentEn,
        string contentAr,
        string? category = null)
    {
        var errors = new List<ValidationError>();

        if (clinicId == Guid.Empty)
            errors.Add(new ValidationError(nameof(clinicId), "ClinicId is required"));

        if (string.IsNullOrWhiteSpace(name))
            errors.Add(new ValidationError(nameof(name), "Name is required"));

        if (string.IsNullOrWhiteSpace(contentEn))
            errors.Add(new ValidationError(nameof(contentEn), "English content is required"));

        if (string.IsNullOrWhiteSpace(contentAr))
            errors.Add(new ValidationError(nameof(contentAr), "Arabic content is required"));

        if (errors.Count > 0)
            return Result<ResponseTemplate>.Invalid(errors);

        return Result<ResponseTemplate>.Success(new ResponseTemplate
        {
            ClinicId = clinicId,
            Name = name,
            ContentEn = contentEn,
            ContentAr = contentAr,
            Category = category
        });
    }

    public Result Update(string name, string contentEn, string contentAr, string? category)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(name))
            errors.Add(new ValidationError(nameof(name), "Name is required"));

        if (string.IsNullOrWhiteSpace(contentEn))
            errors.Add(new ValidationError(nameof(contentEn), "English content is required"));

        if (string.IsNullOrWhiteSpace(contentAr))
            errors.Add(new ValidationError(nameof(contentAr), "Arabic content is required"));

        if (errors.Count > 0)
            return Result.Invalid(errors);

        Name = name;
        ContentEn = contentEn;
        ContentAr = contentAr;
        Category = category;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public ResponseTemplateDto ToDto() => new(
        Id,
        ClinicId,
        Name,
        ContentEn,
        ContentAr,
        Category,
        CreatedAt,
        UpdatedAt);
}
