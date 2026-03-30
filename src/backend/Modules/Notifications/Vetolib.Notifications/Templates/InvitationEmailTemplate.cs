namespace Vetolib.Notifications.Templates;

internal static class InvitationEmailTemplate
{
    public static string Subject(string clinicName, string language = "en") =>
        language == "ar"
            ? $"تمت دعوتك إلى {clinicName}"
            : $"You've been invited to {clinicName}";

    public static string HtmlBody(string fullName, string email, string temporaryPassword, string clinicName, string language = "en") =>
        language == "ar"
            ? $"""
            <p>مرحبا {fullName},</p>
            <p>تمت دعوتك للانضمام إلى <strong>{clinicName}</strong> على Vetara.</p>
            <p><strong>بيانات الدخول المؤقتة:</strong></p>
            <ul>
              <li>البريد الإلكتروني: {email}</li>
              <li>كلمة المرور: {temporaryPassword}</li>
            </ul>
            <p>يرجى تسجيل الدخول وتغيير كلمة المرور فورا.</p>
            <p>مع أطيب التحيات،<br/>فريق {clinicName}</p>
            """
            : $"""
            <p>Hello {fullName},</p>
            <p>You've been invited to join <strong>{clinicName}</strong> on Vetara.</p>
            <p><strong>Your temporary credentials:</strong></p>
            <ul>
              <li>Email: {email}</li>
              <li>Password: {temporaryPassword}</li>
            </ul>
            <p>Please log in and change your password immediately.</p>
            <p>Best regards,<br/>The {clinicName} Team</p>
            """;

    public static string PlainTextBody(string fullName, string email, string temporaryPassword, string clinicName, string language = "en") =>
        language == "ar"
            ? $"""
            مرحبا {fullName},

            تمت دعوتك للانضمام إلى {clinicName} على Vetara.

            بيانات الدخول المؤقتة:
            البريد الإلكتروني: {email}
            كلمة المرور: {temporaryPassword}

            يرجى تسجيل الدخول وتغيير كلمة المرور فورا.

            مع أطيب التحيات،
            فريق {clinicName}
            """
            : $"""
            Hello {fullName},

            You've been invited to join {clinicName} on Vetara.

            Your temporary credentials:
            Email: {email}
            Password: {temporaryPassword}

            Please log in and change your password immediately.

            Best regards,
            The {clinicName} Team
            """;
}
