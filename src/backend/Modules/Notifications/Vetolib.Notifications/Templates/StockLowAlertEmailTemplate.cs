namespace Vetolib.Notifications.Templates;

internal static class StockLowAlertEmailTemplate
{
    public static string Subject(string itemName, string language = "en") =>
        language == "ar"
            ? $"تنبيه مخزون منخفض — {itemName}"
            : $"Low stock alert — {itemName}";

    public static string HtmlBody(
        string itemName,
        int currentQuantity,
        int minThreshold,
        string language = "en") =>
        language == "ar"
            ? $"""
            <p>تنبيه: مخزون <strong>{itemName}</strong> منخفض.</p>
            <p>الكمية الحالية: <strong>{currentQuantity}</strong> (الحد الأدنى: {minThreshold})</p>
            <p>يرجى إعادة التخزين في أقرب وقت ممكن.</p>
            """
            : $"""
            <p>Alert: <strong>{itemName}</strong> is running low.</p>
            <p>Current quantity: <strong>{currentQuantity}</strong> (minimum threshold: {minThreshold})</p>
            <p>Please restock as soon as possible.</p>
            """;

    public static string PlainTextBody(
        string itemName,
        int currentQuantity,
        int minThreshold,
        string language = "en") =>
        language == "ar"
            ? $"""
            تنبيه: مخزون {itemName} منخفض.

            الكمية الحالية: {currentQuantity} (الحد الأدنى: {minThreshold})

            يرجى إعادة التخزين في أقرب وقت ممكن.
            """
            : $"""
            Alert: {itemName} is running low.

            Current quantity: {currentQuantity} (minimum threshold: {minThreshold})

            Please restock as soon as possible.
            """;
}
