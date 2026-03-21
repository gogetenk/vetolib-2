# todo-infra-log-redaction-001 -- Redaction des donnees sensibles dans les logs Serilog

**Module** : Infrastructure / Vetolib.Api
**Priorite** : critique (C-01 security audit -- passwords en clair dans les logs)
**Dependances** : done-infra-monitoring-001
**Skills a lire** : `dotnet-aspire`
**MODIF_SHARED: autorise** (ajout NuGet dans Shared.Infrastructure uniquement)

---

## Contexte

Le security audit a identifie la vulnerabilite C-01 : les seed passwords sont logges en clair
dans `DbInitializer.cs` (lignes 55-58, 65-68). De plus, les emails (PII) sont logges en clair
dans au moins 8 endroits (Notifications consumers, email senders). Il n'existe aucun mecanisme
de redaction automatique dans le pipeline Serilog actuel.

## Perimetre exact

### 1. NuGet a ajouter

Dans `Vetolib.Api.csproj` :
```xml
<PackageReference Include="Microsoft.Extensions.Compliance.Redaction" Version="9.*" />
<PackageReference Include="Serilog.Enrichers.Sensitive" Version="2.*" />
```

> **Note sur le choix technique** : `Microsoft.Extensions.Compliance.Redaction` fournit
> l'infrastructure d'annotations (`[SensitiveData]`, `DataClassification`) et le `RedactorProvider`.
> `Serilog.Enrichers.Sensitive` fournit le destructuring policy Serilog qui utilise ces annotations
> pour redacter automatiquement les proprietes structurees dans les logs.
>
> **Alternative evaluee et ecartee** : ecrire un `IDestructuringPolicy` custom Serilog qui inspecte
> les attributs `[SensitiveData]` via reflection. Ecarte car `Serilog.Enrichers.Sensitive` fait
> exactement cela et est maintenu par la communaute.

### 2. Annotations de donnees sensibles

Creer un fichier `src/backend/Shared/Vetolib.Shared.Kernel/SensitiveDataAttribute.cs` :

```csharp
namespace Vetolib.Shared.Kernel;

/// <summary>
/// Marks a property as containing sensitive data that must be redacted in logs.
/// Used by the Serilog redaction pipeline to automatically mask values.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public sealed class SensitiveDataAttribute : Attribute;
```

> **ATTENTION** : Shared.Kernel est GELE. Cette tache a le flag `MODIF_SHARED: autorise`
> car l'ajout est strictement additif (un nouveau fichier, zero modification de fichiers existants).
> Si le hook `guard-shared.sh` bloque, montrer ce flag.

Annoter les proprietes suivantes :

| Fichier | Propriete | Classification |
|---|---|---|
| `Auth/Domain/User.cs` | `PasswordHash` | Sensitive |
| `Auth/Domain/User.cs` | `Email` | PII |
| `Auth/Commands/Login/LoginCommand.cs` | `Password` | Sensitive |
| `MedicalRecords/Domain/Owner.cs` | `Email` | PII |
| `MedicalRecords/Domain/Owner.cs` | `Phone` | PII |

### 3. Configuration Serilog dans Program.cs

Ajouter dans le bloc `builder.Host.UseSerilog(...)` :

```csharp
config
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "Vetolib.Api")
    .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
    .Enrich.WithSensitiveDataMasking(options =>  // NOUVEAU
    {
        options.MaskingOperators = new List<IMaskingOperator>
        {
            new EmailAddressMaskingOperator(),
            new CreditCardMaskingOperator()
        };
        options.MaskProperties.AddRange(new[]
        {
            "Password", "PasswordHash", "AdminPassword", "VetPassword",
            "RefreshToken", "AccessToken", "Token", "Secret"
        });
    });
```

### 4. Correction immediate de DbInitializer.cs (C-01)

Remplacer les deux `logger.LogWarning` qui loggent les mots de passe :

```csharp
// AVANT (C-01 vulnerability)
logger.LogWarning(
    "Seed:AdminPassword not set. Generated one-time admin password: {AdminPassword} " +
    "-- store this value in a secret manager immediately.",
    adminPassword);

// APRES -- ne plus logger le mot de passe, afficher uniquement en console interactive
logger.LogWarning(
    "Seed:AdminPassword not set. A one-time admin password has been generated. " +
    "Retrieve it from user-secrets or the environment variable Seed__AdminPassword.");

if (context.HostingEnvironment.IsDevelopment())
{
    // Console.WriteLine bypasses Serilog -- acceptable uniquement en dev local
    Console.WriteLine($"[SEED] Admin password: {adminPassword}");
    Console.WriteLine($"[SEED] Vet password: {vetPassword}");
}
```

Meme traitement pour `VetPassword`.

### 5. Redaction dans les log templates existants

Les templates suivants loggent des emails en clair et doivent etre verifies :

| Fichier | Log template | Action |
|---|---|---|
| `Notifications/Consumers/UserInvitedConsumer.cs:43` | `"Invitation email sent to {Email}"` | Sera redacte automatiquement par le MaskingOperator email |
| `Notifications/Consumers/InvoiceSentConsumer.cs:43` | `"Invoice email sent to {Email}"` | Idem |
| `Notifications/Consumers/AppointmentReminderConsumer.cs:44` | `"Reminder email sent to {Email}"` | Idem |
| `Notifications/Infrastructure/MailKitEmailSender.cs:56` | `"Email sent to {To}"` | Ajouter "To" dans MaskProperties |
| `Shared/Infrastructure/Email/SmtpEmailSender.cs:63` | `"Email sent to {To}"` | Idem |
| `Shared/Infrastructure/Email/ConsoleEmailSender.cs:18` | Log complet email | Idem |

## Tests

### Test unitaire (dans un nouveau projet ou fichier de test existant)

```csharp
[Fact]
public void Serilog_redacts_password_from_structured_log()
{
    var output = new StringWriter();
    var logger = new LoggerConfiguration()
        .Enrich.WithSensitiveDataMasking(options =>
        {
            options.MaskProperties.Add("Password");
            options.MaskingOperators.Add(new EmailAddressMaskingOperator());
        })
        .WriteTo.TextWriter(output)
        .CreateLogger();

    logger.Information("Login attempt with {Password} for {Email}",
        "S3cret!Pass", "admin@desertpaws.ae");

    var logOutput = output.ToString();
    Assert.DoesNotContain("S3cret!Pass", logOutput);
    Assert.DoesNotContain("admin@desertpaws.ae", logOutput);
    Assert.Contains("***", logOutput); // masked value
}
```

### Test d'integration (dans Vetolib.Tests.Acceptance)

```csharp
[Fact]
public async Task Seed_passwords_are_not_visible_in_structured_logs()
{
    // Capture Serilog output during app startup
    // Assert that no log entry contains a raw password value
}
```

## Critere de completion

```
[] NuGet Microsoft.Extensions.Compliance.Redaction ajoute
[] NuGet Serilog.Enrichers.Sensitive ajoute
[] SensitiveDataAttribute cree dans Shared.Kernel
[] Annotations PII/Sensitive sur User, Owner, LoginCommand
[] Serilog configure avec WithSensitiveDataMasking dans Program.cs
[] DbInitializer ne logge plus les passwords (C-01 resolu)
[] "To" ajoute dans MaskProperties
[] Test unitaire : un log contenant password+email est redacte
[] Test integration : aucun password brut dans les logs au startup
[] Renommer en done-infra-log-redaction-001.md
```
