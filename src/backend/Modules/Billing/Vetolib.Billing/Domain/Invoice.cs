using Ardalis.Result;
using Vetolib.Billing.Contracts;
using Vetolib.Shared.Kernel;

namespace Vetolib.Billing.Domain;

internal class Invoice : BaseEntity, IMultiTenant, IAggregateRoot
{
    public Guid ClinicId { get; private set; }
    public Guid AnimalId { get; private set; }
    public string InvoiceNumber { get; private set; } = string.Empty;
    public InvoiceStatus Status { get; private set; }
    public DateTime? DueDate { get; private set; }

    private readonly List<InvoiceItem> _items = [];
    public IReadOnlyList<InvoiceItem> Items => _items.AsReadOnly();

    public decimal SubTotal => _items.Sum(i => i.UnitPriceExclTax);
    public decimal TotalTax => _items.Sum(i => i.TaxAmount);
    public decimal Total => _items.Sum(i => i.TotalInclTax);

    private Invoice() { } // EF Core

    public static Result<Invoice> Create(
        Guid clinicId,
        Guid animalId,
        string invoiceNumber,
        string itemDescription,
        decimal itemUnitPrice,
        decimal taxRate = 0.05m)
    {
        var errors = new List<ValidationError>();

        if (clinicId == Guid.Empty)
            errors.Add(new ValidationError(nameof(clinicId), "ClinicId is required"));

        if (animalId == Guid.Empty)
            errors.Add(new ValidationError(nameof(animalId), "AnimalId is required"));

        if (string.IsNullOrWhiteSpace(invoiceNumber))
            errors.Add(new ValidationError(nameof(invoiceNumber), "Invoice number is required"));

        if (errors.Count > 0)
            return Result<Invoice>.Invalid(errors);

        var invoice = new Invoice
        {
            ClinicId = clinicId,
            AnimalId = animalId,
            InvoiceNumber = invoiceNumber,
            Status = InvoiceStatus.Draft
        };

        // Add the initial item
        var itemResult = InvoiceItem.Create(invoice.Id, itemDescription, itemUnitPrice, taxRate);
        if (!itemResult.IsSuccess)
            return Result<Invoice>.Invalid(itemResult.ValidationErrors.ToList());

        invoice._items.Add(itemResult.Value);

        return Result<Invoice>.Success(invoice);
    }

    public Result<InvoiceItem> AddItem(string description, decimal unitPrice, decimal taxRate = 0.05m)
    {
        if (Status == InvoiceStatus.Paid)
            return Result<InvoiceItem>.Error("INVOICE_IMMUTABLE:A paid invoice cannot be modified");

        if (Status == InvoiceStatus.Cancelled)
            return Result<InvoiceItem>.Error("INVOICE_CANCELLED:A cancelled invoice cannot be modified");

        var itemResult = InvoiceItem.Create(Id, description, unitPrice, taxRate);
        if (!itemResult.IsSuccess)
            return itemResult;

        _items.Add(itemResult.Value);
        return itemResult;
    }

    public Result UpdateStatus(InvoiceStatus newStatus, int dueDateDays = 30)
    {
        if (Status == InvoiceStatus.Paid)
            return Result.Error("INVOICE_IMMUTABLE:A paid invoice cannot be modified");

        if (Status == InvoiceStatus.Cancelled)
            return Result.Error("INVOICE_CANCELLED:A cancelled invoice cannot be modified");

        var validTransition = (Status, newStatus) switch
        {
            (InvoiceStatus.Draft, InvoiceStatus.Sent) => true,
            (InvoiceStatus.Draft, InvoiceStatus.Cancelled) => true,
            (InvoiceStatus.Sent, InvoiceStatus.Paid) => true,
            (InvoiceStatus.Sent, InvoiceStatus.Cancelled) => true,
            _ => false
        };

        if (!validTransition)
            return Result.Error(
                $"INVALID_TRANSITION:Transition from {Status} to {newStatus} is not allowed");

        if (newStatus == InvoiceStatus.Sent && _items.Count == 0)
            return Result.Error("INVOICE_EMPTY:Invoice must contain at least one item");

        Status = newStatus;

        if (newStatus == InvoiceStatus.Sent)
            DueDate = DateTime.UtcNow.AddDays(dueDateDays);

        return Result.Success();
    }

    public InvoiceDto ToDto(decimal vatRate = 0.05m) => new(
        Id,
        InvoiceNumber,
        AnimalId,
        PatientName: null,
        OwnerName: null,
        OwnerPhone: null,
        AppointmentId: null,
        Status,
        _items.Select(i => i.ToDto()).ToList().AsReadOnly(),
        Subtotal: SubTotal,
        VatRate: vatRate,
        VatAmount: TotalTax,
        Total,
        Notes: null,
        CreatedAt,
        PaidAt: null,
        DueDate,
        ClinicId);
}
