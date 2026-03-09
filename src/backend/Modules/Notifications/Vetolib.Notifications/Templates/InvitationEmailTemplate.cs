namespace Vetolib.Notifications.Templates;

internal static class InvitationEmailTemplate
{
    public static string Subject(string clinicName) =>
        $"You've been invited to {clinicName}";

    public static string HtmlBody(string fullName, string email, string temporaryPassword, string clinicName) => $"""
        <p>Hello {fullName},</p>
        <p>You've been invited to join <strong>{clinicName}</strong> on Vetolib.</p>
        <p><strong>Your temporary credentials:</strong></p>
        <ul>
          <li>Email: {email}</li>
          <li>Password: {temporaryPassword}</li>
        </ul>
        <p>Please log in and change your password immediately.</p>
        <p>Best regards,<br/>The {clinicName} Team</p>
        """;

    public static string PlainTextBody(string fullName, string email, string temporaryPassword, string clinicName) => $"""
        Hello {fullName},

        You've been invited to join {clinicName} on Vetolib.

        Your temporary credentials:
        Email: {email}
        Password: {temporaryPassword}

        Please log in and change your password immediately.

        Best regards,
        The {clinicName} Team
        """;
}
