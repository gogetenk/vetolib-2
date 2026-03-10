# todo-back-notifications-001.md — Backend : Module Notifications (MassTransit Outbox + MailHog)

**Dépendances** : done-scaffold-000
**Skills** : `ardalis-result`, `cqrs-mediatr`, `ardalis-modular-monolith`, `dotnet-aspire`

---

## Objectif

Créer un module `Vetolib.Notifications` centralisé qui reçoit des Domain Events via MassTransit
et envoie des notifications (email pour le MVP). Pattern Outbox pour garantir la livraison.

Canaux MVP : **Email uniquement via MailHog** (développement) / SMTP (production).
Canaux V2 : Push (Firebase), SMS (Twilio).

---

## Stack

```
NuGet :
  MassTransit (14.*)
  MassTransit.EntityFrameworkCore      ← Outbox pattern
  MassTransit.RabbitMQ                 ← ou InMemory pour le dev
  MailKit                              ← envoi SMTP
  MimeKit                              ← construction des emails
```

## Structure

```
Modules/Notifications/
├── Vetolib.Notifications.Contracts/
│   ├── Events/
│   │   ├── SendEmailRequest.cs        ← public record
│   │   ├── AppointmentReminderEvent.cs
│   │   ├── InvoiceSentEvent.cs
│   │   └── UserInvitedEvent.cs
│   └── Enums/
│       └── NotificationChannel.cs    ← Email | Push | Sms
└── Vetolib.Notifications/
    ├── Consumers/
    │   ├── SendEmailConsumer.cs       ← consomme SendEmailRequest
    │   ├── AppointmentReminderConsumer.cs
    │   ├── InvoiceSentConsumer.cs
    │   └── UserInvitedConsumer.cs
    ├── Infrastructure/
    │   ├── NotificationsDbContext.cs  ← pour l'Outbox EF Core
    │   └── MailKitEmailSender.cs      ← implémente IEmailSender
    ├── Templates/
    │   ├── appointment-reminder.en.html
    │   ├── appointment-reminder.ar.html
    │   ├── invoice-sent.en.html
    │   ├── invoice-sent.ar.html
    │   ├── user-invited.en.html
    │   └── user-invited.ar.html
    └── NotificationsModuleServiceRegistrar.cs
```

## Outbox Pattern

```csharp
// NotificationsModuleServiceRegistrar.cs
services.AddMassTransit(x =>
{
    x.AddEntityFrameworkOutbox<NotificationsDbContext>(o =>
    {
        o.UsePostgres();
        o.UseBusOutbox();
    });

    x.AddConsumer<SendEmailConsumer>();
    x.AddConsumer<AppointmentReminderConsumer>();
    x.AddConsumer<InvoiceSentConsumer>();
    x.AddConsumer<UserInvitedConsumer>();

    x.UsingRabbitMq((ctx, cfg) =>  // ou InMemory si pas de RabbitMQ
    {
        cfg.ConfigureEndpoints(ctx);
    });
});
```

## Contrats (comment les autres modules publient)

```csharp
// Depuis n'importe quel handler, via MassTransit IBus :
await _bus.Publish(new UserInvitedEvent(
    Email: "dr.sarah@desertpaws.ae",
    FullName: "Dr. Sarah Johnson",
    TemporaryPassword: "Abc12345",
    PreferredLanguage: "en"  // ou "ar"
));

// Ou via SendEmailRequest générique :
await _bus.Publish(new SendEmailRequest(
    To: "patient@example.com",
    TemplateKey: "appointment-reminder",
    Language: "ar",
    Data: new { PatientName = "ماكس", VetName = "د. سارة", DateTime = "..." }
));
```

## Configuration MailHog (développement)

```json
// appsettings.Development.json
{
  "Notifications": {
    "Smtp": {
      "Host": "localhost",
      "Port": 1025,
      "From": "noreply@desertpaws.ae",
      "UseSsl": false
    }
  }
}
```

```yaml
# docker-compose.yml (ajouter)
mailhog:
  image: mailhog/mailhog
  ports:
    - "1025:1025"   # SMTP
    - "8025:8025"   # UI web → http://localhost:8025
```

```csharp
// AppHost/Program.cs (ajouter)
var mailhog = builder.AddContainer("mailhog", "mailhog/mailhog")
    .WithEndpoint(1025, 1025, name: "smtp")
    .WithEndpoint(8025, 8025, name: "ui");
```

## Templates bilingues

Chaque template existe en `.en.html` et `.ar.html`.
Le consumer choisit selon `PreferredLanguage` (défaut : "en").
Les templates `.ar.html` ont `dir="rtl"` sur le body.

Template minimal `user-invited.en.html` :
```html
<!DOCTYPE html>
<html lang="en">
<body>
  <h1>Welcome to {{ClinicName}}</h1>
  <p>Your account has been created.</p>
  <p>Email: {{Email}}</p>
  <p>Temporary password: <strong>{{TemporaryPassword}}</strong></p>
  <p>Please change your password after first login.</p>
</body>
</html>
```

Template minimal `user-invited.ar.html` :
```html
<!DOCTYPE html>
<html lang="ar" dir="rtl">
<body>
  <h1>مرحباً بك في {{ClinicName}}</h1>
  <p>تم إنشاء حسابك.</p>
  <p>البريد الإلكتروني: {{Email}}</p>
  <p>كلمة المرور المؤقتة: <strong>{{TemporaryPassword}}</strong></p>
  <p>يرجى تغيير كلمة المرور بعد تسجيل الدخول الأول.</p>
</body>
</html>
```

## Notifications à implémenter dans le MVP

| Événement | Template | Destinataire |
|---|---|---|
| Utilisateur invité | `user-invited` | Nouvel utilisateur |
| RDV confirmé | `appointment-reminder` | Propriétaire animal |
| Facture envoyée | `invoice-sent` | Propriétaire animal |

## Critère de complétion

```
□ MassTransit + Outbox configurés et fonctionnels
□ MailHog accessible sur http://localhost:8025
□ Aspire AppHost orchestre MailHog
□ UserInvitedEvent → email reçu dans MailHog (vérifiable)
□ Templates bilingues EN + AR présents
□ dotnet build → 0 erreur
□ Renommer en done-back-notifications-001.md
```
