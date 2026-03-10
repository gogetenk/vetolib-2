namespace Vetolib.Billing.Contracts;

public record InvoiceDto(
    Guid Id,
    string InvoiceNumber,
    Guid? PatientId,
    string? PatientName,
    string? OwnerName,
    string? OwnerPhone,
    Guid? AppointmentId,
    InvoiceStatus Status,
    IReadOnlyList<InvoiceItemDto> Items,
    decimal Subtotal,
    decimal VatRate,
    decimal VatAmount,
    decimal Total,
    string? Notes,
    DateTime CreatedAt,
    DateTime? PaidAt,
    DateTime? DueDate,
    Guid ClinicId);
