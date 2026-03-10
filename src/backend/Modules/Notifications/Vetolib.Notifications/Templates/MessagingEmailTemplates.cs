namespace Vetolib.Notifications.Templates;

internal static class MessagingEmailTemplates
{
    public static class OwnerMessageReply
    {
        public static string Subject() => "You have a new message from your veterinary clinic";

        public static string HtmlBody(string portalUrl) =>
            $"""
            <p>Hello,</p>
            <p>Your veterinary clinic has replied to your message.</p>
            <p><a href="{portalUrl}">Click here to view the reply</a></p>
            <p>If you did not expect this message, please ignore it.</p>
            """;

        public static string PlainTextBody(string portalUrl) =>
            $"""
            Hello,

            Your veterinary clinic has replied to your message.

            View the reply here: {portalUrl}

            If you did not expect this message, please ignore it.
            """;
    }

    public static class EmergencyMessageReceived
    {
        public static string Subject() => "URGENT: Emergency message received from a pet owner";

        public static string HtmlBody(string messagePreview, Guid conversationId) =>
            $"""
            <p><strong>An emergency message has been received and requires immediate attention.</strong></p>
            <p>Message preview: {messagePreview}</p>
            <p>Conversation ID: {conversationId}</p>
            <p>Please log in to the clinic portal to respond.</p>
            """;

        public static string PlainTextBody(string messagePreview, Guid conversationId) =>
            $"""
            URGENT: An emergency message has been received and requires immediate attention.

            Message preview: {messagePreview}
            Conversation ID: {conversationId}

            Please log in to the clinic portal to respond.
            """;
    }

    public static class EmergencyEscalation
    {
        public static string Subject() => "URGENT — Unread emergency message requires immediate action";

        public static string HtmlBody(string messagePreview, Guid conversationId) =>
            $"""
            <p><strong>URGENT: An emergency message has not been responded to and requires immediate attention.</strong></p>
            <p>Message preview: {messagePreview}</p>
            <p>Conversation ID: {conversationId}</p>
            <p>This conversation has been open for more than 10 minutes without a response. Please act immediately.</p>
            """;

        public static string PlainTextBody(string messagePreview, Guid conversationId) =>
            $"""
            URGENT — An emergency message has not been responded to.

            Message preview: {messagePreview}
            Conversation ID: {conversationId}

            This conversation has been open for more than 10 minutes without a response.
            Please log in immediately to the clinic portal to respond.
            """;
    }

    public static class MagicLink
    {
        public static string Subject() => "Your secure access link to the pet owner portal";

        public static string HtmlBody(string portalUrl) =>
            $"""
            <p>Hello,</p>
            <p>You requested access to the pet owner portal. Click the link below to sign in securely:</p>
            <p><a href="{portalUrl}">Access the portal</a></p>
            <p>This link is valid for a limited time and can only be used once.</p>
            <p>If you did not request this link, please ignore this email.</p>
            """;

        public static string PlainTextBody(string portalUrl) =>
            $"""
            Hello,

            You requested access to the pet owner portal. Use the link below to sign in securely:

            {portalUrl}

            This link is valid for a limited time and can only be used once.
            If you did not request this link, please ignore this email.
            """;
    }

    public static class OutboundConversation
    {
        public static string Subject() => "You have a new message from your veterinary clinic";

        public static string HtmlBody(string messagePreview, string portalUrl) =>
            $"""
            <p>Hello,</p>
            <p>Your veterinary clinic has sent you a new message.</p>
            <p>Message preview: {messagePreview}</p>
            <p><a href="{portalUrl}">Click here to read the full message and reply</a></p>
            """;

        public static string PlainTextBody(string messagePreview, string portalUrl) =>
            $"""
            Hello,

            Your veterinary clinic has sent you a new message.

            Message preview: {messagePreview}

            Read the full message and reply here: {portalUrl}
            """;
    }
}
