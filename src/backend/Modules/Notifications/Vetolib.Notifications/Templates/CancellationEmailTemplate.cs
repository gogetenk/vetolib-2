namespace Vetolib.Notifications.Templates;

internal static class CancellationEmailTemplate
{
    public static string Subject(string patientName, string language = "en") =>
        language == "ar"
            ? $"تم إلغاء الموعد — {patientName}"
            : $"Appointment cancelled — {patientName}";

    public static string HtmlBody(string ownerName, string patientName, string vetName, DateTime scheduledAt, string clinicName, string? reason, string language = "en") =>
        language == "ar"
            ? $"""
            <p>مرحبا {ownerName},</p>
            <p>نود إعلامك بأن موعد <strong>{patientName}</strong> المقرر في
            <strong>{scheduledAt:dd/MM/yyyy}</strong> الساعة <strong>{scheduledAt:HH:mm}</strong>
            مع الطبيب البيطري {vetName} قد تم إلغاؤه.</p>
            {(reason is not null ? $"<p>السبب: {reason}</p>" : "")}
            <p>إذا كنت ترغب في إعادة الحجز، يرجى التواصل معنا.</p>
            <p>مع أطيب التحيات،<br/>{clinicName}</p>
            """
            : $"""
            <p>Hello {ownerName},</p>
            <p>We are writing to let you know that the appointment for <strong>{patientName}</strong> scheduled on
            <strong>{scheduledAt:dd/MM/yyyy}</strong> at <strong>{scheduledAt:HH:mm}</strong>
            with {vetName} has been cancelled.</p>
            {(reason is not null ? $"<p>Reason: {reason}</p>" : "")}
            <p>If you would like to rebook, please contact us.</p>
            <p>Best regards,<br/>{clinicName}</p>
            """;

    public static string PlainTextBody(string ownerName, string patientName, string vetName, DateTime scheduledAt, string clinicName, string? reason, string language = "en") =>
        language == "ar"
            ? $"""
            مرحبا {ownerName},

            نود إعلامك بأن موعد {patientName} المقرر في {scheduledAt:dd/MM/yyyy} الساعة {scheduledAt:HH:mm} مع الطبيب البيطري {vetName} قد تم إلغاؤه.
            {(reason is not null ? $"السبب: {reason}" : "")}

            إذا كنت ترغب في إعادة الحجز، يرجى التواصل معنا.

            مع أطيب التحيات،
            {clinicName}
            """
            : $"""
            Hello {ownerName},

            We are writing to let you know that the appointment for {patientName} scheduled on {scheduledAt:dd/MM/yyyy} at {scheduledAt:HH:mm} with {vetName} has been cancelled.
            {(reason is not null ? $"Reason: {reason}" : "")}

            If you would like to rebook, please contact us.

            Best regards,
            {clinicName}
            """;
}
