using System.Text.Json.Serialization;

namespace Vetolib.Messaging.Contracts;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ConversationChannel
{
    Portal,
    WhatsApp,
    Sms
}
