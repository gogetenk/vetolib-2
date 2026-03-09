namespace Vetolib.Notifications.Templates;

internal static class InvoiceEmailTemplate
{
    public static string Subject(string invoiceNumber, string clinicName) =>
        $"Invoice {invoiceNumber} from {clinicName}";

    public static string HtmlBody(string ownerName, string invoiceNumber, decimal totalAmount, string currency, string clinicName) => $"""
        <p>Hello {ownerName},</p>
        <p>Please find attached your invoice <strong>{invoiceNumber}</strong> from <strong>{clinicName}</strong>.</p>
        <p><strong>Total amount: {totalAmount:F2} {currency}</strong></p>
        <p>Thank you for trusting us with your pet's care.</p>
        <p>Best regards,<br/>{clinicName}</p>
        """;

    public static string PlainTextBody(string ownerName, string invoiceNumber, decimal totalAmount, string currency, string clinicName) => $"""
        Hello {ownerName},

        Please find your invoice {invoiceNumber} from {clinicName}.

        Total amount: {totalAmount:F2} {currency}

        Thank you for trusting us with your pet's care.

        Best regards,
        {clinicName}
        """;
}
