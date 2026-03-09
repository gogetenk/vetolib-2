namespace Vetolib.Billing.Contracts;

public record InvoiceDto(
    Guid Id,
    string InvoiceNumber,
    Guid AnimalId,
    InvoiceStatus Status,
    decimal SubTotal,
    decimal TotalTax,
    decimal Total,
    DateTime? DueDate,
    IReadOnlyList<InvoiceItemDto> Items,
    DateTime CreatedAt);
