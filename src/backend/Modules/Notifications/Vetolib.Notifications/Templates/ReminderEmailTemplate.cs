namespace Vetolib.Notifications.Templates;

internal static class ReminderEmailTemplate
{
    public static string Subject(string patientName, string time, string language = "en") =>
        language == "ar"
            ? $"تذكير بالموعد — {patientName} غدا الساعة {time}"
            : $"Appointment reminder — {patientName} tomorrow at {time}";

    public static string HtmlBody(string ownerName, string patientName, string vetName, DateTime scheduledAt, string language = "en") =>
        language == "ar"
            ? $"""
            <p>مرحبا {ownerName},</p>
            <p>هذا تذكير بأن <strong>{patientName}</strong> لديه موعد
            غدا الساعة <strong>{scheduledAt:HH:mm}</strong>.</p>
            <p>الطبيب البيطري: {vetName}</p>
            <p>إذا كنت بحاجة لإعادة الجدولة، يرجى التواصل معنا.</p>
            """
            : $"""
            <p>Hello {ownerName},</p>
            <p>This is a reminder that <strong>{patientName}</strong> has an appointment
            tomorrow at <strong>{scheduledAt:HH:mm}</strong>.</p>
            <p>Veterinarian: {vetName}</p>
            <p>If you need to reschedule, please contact us.</p>
            """;

    public static string PlainTextBody(string ownerName, string patientName, string vetName, DateTime scheduledAt, string language = "en") =>
        language == "ar"
            ? $"""
            مرحبا {ownerName},

            هذا تذكير بأن {patientName} لديه موعد غدا الساعة {scheduledAt:HH:mm}.

            الطبيب البيطري: {vetName}

            إذا كنت بحاجة لإعادة الجدولة، يرجى التواصل معنا.
            """
            : $"""
            Hello {ownerName},

            This is a reminder that {patientName} has an appointment
            tomorrow at {scheduledAt:HH:mm}.

            Veterinarian: {vetName}

            If you need to reschedule, please contact us.
            """;
}
