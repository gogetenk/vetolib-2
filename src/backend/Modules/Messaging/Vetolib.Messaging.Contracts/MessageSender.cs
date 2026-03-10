namespace Vetolib.Messaging.Contracts;

public enum MessageSender
{
    Owner,      // Pet owner via portal
    Vet,        // Veterinarian reply
    Staff,      // Receptionist or Admin reply
    System      // Auto-acknowledgment (out-of-hours)
}
