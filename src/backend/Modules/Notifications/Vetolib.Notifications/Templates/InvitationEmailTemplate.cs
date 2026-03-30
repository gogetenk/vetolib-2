namespace Vetolib.Notifications.Templates;

internal static class InvitationEmailTemplate
{
    public static string Subject(string clinicName, string language = "en") =>
        language == "ar"
            ? $"تمت دعوتك إلى {clinicName}"
            : $"You've been invited to {clinicName}";

    public static string HtmlBody(string fullName, string email, string clinicName, string language = "en") =>
        language == "ar"
            ? $"""
            <p>مرحبا {fullName},</p>
            <p>تمت دعوتك للانضمام إلى <strong>{clinicName}</strong> على Vetara.</p>
            <p><strong>بيانات الدخول:</strong></p>
            <ul>
              <li>البريد الإلكتروني: {email}</li>
            </ul>
            <p>يرجى استخدام خيار "نسيت كلمة المرور" لتعيين كلمة المرور الخاصة بك عند أول تسجيل دخول.</p>
            <p>مع أطيب التحيات،<br/>فريق {clinicName}</p>
            """
            : $"""
            <p>Hello {fullName},</p>
            <p>You've been invited to join <strong>{clinicName}</strong> on Vetara.</p>
            <p><strong>Your login details:</strong></p>
            <ul>
              <li>Email: {email}</li>
            </ul>
            <p>Please use the "Forgot Password" option to set your password on your first login.</p>
            <p>Best regards,<br/>The {clinicName} Team</p>
            """;

    public static string PlainTextBody(string fullName, string email, string clinicName, string language = "en") =>
        language == "ar"
            ? $"""
            مرحبا {fullName},

            تمت دعوتك للانضمام إلى {clinicName} على Vetara.

            بيانات الدخول:
            البريد الإلكتروني: {email}

            يرجى استخدام خيار "نسيت كلمة المرور" لتعيين كلمة المرور الخاصة بك عند أول تسجيل دخول.

            مع أطيب التحيات،
            فريق {clinicName}
            """
            : $"""
            Hello {fullName},

            You've been invited to join {clinicName} on Vetara.

            Your login details:
            Email: {email}

            Please use the "Forgot Password" option to set your password on your first login.

            Best regards,
            The {clinicName} Team
            """;
}
