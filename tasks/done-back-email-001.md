# todo-back-email-001.md — Système d'email (invitation + rappels RDV)

**Module** : Shared (service email) + Auth (invitation) + Agenda (rappels)
**Dépendances** : done-back-migrations-001
**Skills à lire** : `ardalis-result`, `cqrs-mediatr`
**MODIF_SHARED: autorisé** (ajout d'un service email dans Shared.Infrastructure)

---

## Contexte

L'invitation utilisateur retourne un mot de passe temporaire à l'écran mais n'envoie pas d'email. Les RDV n'ont aucun rappel. Pour une clinique réelle, c'est inutilisable.

## Périmètre exact

### 1. Service email abstrait dans Shared

Créer une interface dans `Shared.Kernel` et une implémentation dans `Shared.Infrastructure` :

```csharp
// Shared.Kernel
public interface IEmailSender
{
    Task<Result> SendAsync(EmailMessage message, CancellationToken ct = default);
}

public record EmailMessage(
    string To,
    string Subject,
    string HtmlBody,
    string? PlainTextBody = null);
```

Implémentation via **SMTP** (configurable) avec un provider gratuit type :
- Resend (API simple, 3000 emails/mois gratuit)
- Ou SMTP générique (Mailhog en dev, vrai SMTP en prod)

**En dev** : utiliser un `ConsoleEmailSender` qui log l'email dans la console (pas d'envoi réel). Configurable via :
```json
"Email": {
  "Provider": "console"  // ou "smtp" ou "resend"
}
```

### 2. Email d'invitation utilisateur

Quand `InviteUserHandler` crée un compte, envoyer un email :

```
Subject: You've been invited to Desert Paws Veterinary Clinic
Body:
  Hello {FullName},

  You've been invited to join {ClinicName} on Vetolib.

  Your temporary credentials:
  Email: {email}
  Password: {tempPassword}

  Please log in and change your password.

  {loginUrl}
```

L'envoi d'email ne doit PAS bloquer la réponse HTTP. Utiliser un MediatR notification (fire-and-forget) :

```csharp
// Dans InviteUserHandler, après création :
await _mediator.Publish(new UserInvitedNotification(user.Email, tempPassword, clinicName));

// Handler séparé qui envoie l'email
internal class SendInviteEmailHandler : INotificationHandler<UserInvitedNotification>
```

### 3. Rappels RDV (24h avant)

Créer un **background service** (IHostedService) qui tourne toutes les heures :

```csharp
internal class AppointmentReminderService : BackgroundService
{
    // Toutes les heures :
    // 1. Chercher les RDV avec statut SCHEDULED dont la date est dans 23-25h
    // 2. Pour chaque RDV non encore notifié (flag reminder_sent)
    // 3. Envoyer un email au propriétaire
    // 4. Marquer reminder_sent = true
}
```

Email de rappel :
```
Subject: Appointment reminder — {PatientName} tomorrow at {Time}
Body:
  Hello {OwnerName},

  This is a reminder that {PatientName} has an appointment
  tomorrow at {Time} at {ClinicName}.

  Veterinarian: {VetName}

  If you need to reschedule, please call us at {ClinicPhone}.
```

### 4. Ajouter le champ owner email

Actuellement `Owner` a `Name` + `Phone` mais pas d'email. Ajouter :
- `Owner.Email` (optionnel)
- Migration EF pour le nouveau champ
- Le rappel n'est envoyé que si l'owner a un email

## Critère de complétion

```
□ IEmailSender dans Shared.Kernel
□ ConsoleEmailSender pour le dev (log dans la console)
□ SmtpEmailSender ou ResendEmailSender pour la prod
□ Email d'invitation envoyé à la création d'utilisateur
□ Background service rappels RDV (24h avant)
□ Owner.Email ajouté + migration
□ Rappel envoyé uniquement si email présent
□ Tests unitaires : handlers de notification
□ Renommer en done-back-email-001.md
```
