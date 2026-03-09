namespace Vetolib.Notifications.Templates;

internal static class ReminderEmailTemplate
{
    public static string Subject(string patientName, string time) =>
        $"Appointment reminder — {patientName} tomorrow at {time}";

    public static string HtmlBody(string ownerName, string patientName, string vetName, DateTime scheduledAt) =>
        $"""
        <p>Hello {ownerName},</p>
        <p>This is a reminder that <strong>{patientName}</strong> has an appointment
        tomorrow at <strong>{scheduledAt:HH:mm}</strong>.</p>
        <p>Veterinarian: {vetName}</p>
        <p>If you need to reschedule, please contact us.</p>
        """;

    public static string PlainTextBody(string ownerName, string patientName, string vetName, DateTime scheduledAt) =>
        $"""
        Hello {ownerName},

        This is a reminder that {patientName} has an appointment
        tomorrow at {scheduledAt:HH:mm}.

        Veterinarian: {vetName}

        If you need to reschedule, please contact us.
        """;
}
