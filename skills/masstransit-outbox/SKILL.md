# MassTransit + Outbox Pattern

## Concepts clés

MassTransit est un bus de messages pour .NET. Dans Vetolib, il sert à :
1. **Découpler** les modules — un module publie un event, Notifications le consomme
2. **Garantir la livraison** via le pattern Outbox (EF Core)
3. **Éviter les appels directs** entre modules pour les side effects

## Setup standard dans Vetolib

```csharp
// NotificationsModuleServiceRegistrar.cs
services.AddMassTransit(x =>
{
    // Outbox : garantit "at least once delivery" même si le process crash
    x.AddEntityFrameworkOutbox<NotificationsDbContext>(o =>
    {
        o.UsePostgres();
        o.UseBusOutbox();           // intègre l'outbox dans EF SaveChanges
        o.QueryDelay = TimeSpan.FromSeconds(1);
    });

    x.AddConsumer<SendEmailConsumer>();
    x.AddConsumer<AppointmentReminderConsumer>();
    x.AddConsumer<InvoiceSentConsumer>();
    x.AddConsumer<UserInvitedConsumer>();

    // Dev : InMemory (pas de RabbitMQ requis)
    // Prod : RabbitMQ
    if (env.IsDevelopment())
        x.UsingInMemory((ctx, cfg) => cfg.ConfigureEndpoints(ctx));
    else
        x.UsingRabbitMq((ctx, cfg) =>
        {
            cfg.Host(config["RabbitMQ:Host"]);
            cfg.ConfigureEndpoints(ctx);
        });
});
```

## Publier un event (depuis n'importe quel handler)

```csharp
// Injection : IBus _bus
// Dans un MediatR CommandHandler après succès :

await _bus.Publish(new UserInvitedEvent(
    Email: user.Email,
    FullName: user.FullName,
    TemporaryPassword: tempPassword,
    PreferredLanguage: user.PreferredLanguage ?? "en"
), cancellationToken);

// IMPORTANT : si l'Outbox est configuré, Publish() ne fait qu'insérer
// dans la table OutboxMessage (dans la même transaction EF).
// Le message est envoyé APRÈS que SaveChanges() réussit.
// Pas de risque d'envoyer un email pour une transaction qui rollback.
```

## Consumer standard

```csharp
public class UserInvitedConsumer : IConsumer<UserInvitedEvent>
{
    private readonly IEmailSender _emailSender;

    public UserInvitedConsumer(IEmailSender emailSender)
        => _emailSender = emailSender;

    public async Task Consume(ConsumeContext<UserInvitedEvent> context)
    {
        var msg = context.Message;
        await _emailSender.SendAsync(
            to: msg.Email,
            templateKey: "user-invited",
            language: msg.PreferredLanguage,
            data: new { msg.FullName, msg.TemporaryPassword }
        );
    }
}
```

## Outbox — comment ça marche

```
1. Handler appelle _bus.Publish(event) dans la transaction
2. MassTransit écrit dans OutboxMessage (même DB, même transaction)
3. Handler appelle _dbContext.SaveChangesAsync()
4. Transaction committée → OutboxMessage committée aussi
5. Background job (OutboxDeliveryService) lit les OutboxMessages
6. Envoie vers le bus (InMemory ou RabbitMQ)
7. Consumer reçoit → envoie email via MailKit
8. OutboxMessage marquée comme delivered
```

Avantage : même si le process crash entre 3 et 5, le message sera
réenvoyé au redémarrage. L'email ne peut pas être "oublié".

## MailKit + MailHog

```csharp
// MailKitEmailSender.cs
public class MailKitEmailSender : IEmailSender
{
    private readonly SmtpConfig _config;  // Host, Port, From, UseSsl

    public async Task SendAsync(string to, string templateKey, string language, object data)
    {
        var templatePath = $"Templates/{templateKey}.{language}.html";
        var html = await File.ReadAllTextAsync(templatePath);
        html = ApplyHandlebars(html, data);  // simple string.Replace

        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(_config.From));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = GetSubject(templateKey, language);
        message.Body = new TextPart(TextFormat.Html) { Text = html };

        using var client = new SmtpClient();
        await client.ConnectAsync(_config.Host, _config.Port, _config.UseSsl);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
```

```json
// appsettings.Development.json
{ "Notifications": { "Smtp": { "Host": "localhost", "Port": 1025, "UseSsl": false } } }
```

## Anti-patterns à éviter

```
❌ Appeler IEmailSender directement depuis un CommandHandler
   → Toujours passer par le bus (Outbox garantie)

❌ Mettre la logique métier dans un Consumer
   → Les Consumers font UNE chose : envoyer la notification

❌ Publier des events depuis le domaine (Domain Events MediatR ≠ Bus events)
   → Les Domain Events MediatR restent intra-module
   → Le handler publie sur le bus APRÈS avoir traité l'event MediatR

❌ Oublier ConfigureEndpoints()
   → Sans ça, MassTransit ne crée pas les queues automatiquement

❌ Utiliser IBus en dehors d'un SaveChanges pour profiter de l'Outbox
   → L'Outbox ne fonctionne que dans une transaction EF Core active
```

## Tests

```csharp
// Tester un consumer directement (sans bus)
var consumer = new UserInvitedConsumer(mockEmailSender);
var context = Mock.Of<ConsumeContext<UserInvitedEvent>>(c =>
    c.Message == new UserInvitedEvent("test@example.com", "Test", "Abc@123", "en"));

await consumer.Consume(context);

mockEmailSender.Verify(x => x.SendAsync("test@example.com", "user-invited", "en", It.IsAny<object>()));
```

## Aspire + RabbitMQ (production)

```csharp
// AppHost/Program.cs
var rabbitmq = builder.AddRabbitMQ("rabbitmq")
    .WithManagementPlugin();  // UI → http://localhost:15672

var api = builder.AddProject<Projects.Vetolib_Api>("api")
    .WithReference(rabbitmq);
```
