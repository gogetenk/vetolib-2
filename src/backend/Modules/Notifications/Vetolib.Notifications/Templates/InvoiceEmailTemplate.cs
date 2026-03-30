namespace Vetolib.Notifications.Templates;

internal static class InvoiceEmailTemplate
{
    public static string Subject(string invoiceNumber, string clinicName, string language = "en") =>
        language == "ar"
            ? $"فاتورة {invoiceNumber} من {clinicName}"
            : $"Invoice {invoiceNumber} from {clinicName}";

    public static string HtmlBody(string ownerName, string invoiceNumber, decimal totalAmount, string currency, string clinicName, string language = "en") =>
        language == "ar"
            ? $"""
            <p>مرحبا {ownerName},</p>
            <p>يرجى الاطلاع على فاتورتكم <strong>{invoiceNumber}</strong> من <strong>{clinicName}</strong>.</p>
            <p><strong>المبلغ الإجمالي: {totalAmount:F2} {currency}</strong></p>
            <p>شكرا لثقتكم بنا في رعاية حيوانكم الأليف.</p>
            <p>مع أطيب التحيات،<br/>{clinicName}</p>
            """
            : $"""
            <p>Hello {ownerName},</p>
            <p>Please find attached your invoice <strong>{invoiceNumber}</strong> from <strong>{clinicName}</strong>.</p>
            <p><strong>Total amount: {totalAmount:F2} {currency}</strong></p>
            <p>Thank you for trusting us with your pet's care.</p>
            <p>Best regards,<br/>{clinicName}</p>
            """;

    public static string PlainTextBody(string ownerName, string invoiceNumber, decimal totalAmount, string currency, string clinicName, string language = "en") =>
        language == "ar"
            ? $"""
            مرحبا {ownerName},

            يرجى الاطلاع على فاتورتكم {invoiceNumber} من {clinicName}.

            المبلغ الإجمالي: {totalAmount:F2} {currency}

            شكرا لثقتكم بنا في رعاية حيوانكم الأليف.

            مع أطيب التحيات،
            {clinicName}
            """
            : $"""
            Hello {ownerName},

            Please find your invoice {invoiceNumber} from {clinicName}.

            Total amount: {totalAmount:F2} {currency}

            Thank you for trusting us with your pet's care.

            Best regards,
            {clinicName}
            """;
}
