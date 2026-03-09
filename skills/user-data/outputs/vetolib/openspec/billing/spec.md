# openspec/billing/spec.md — Module Billing

## Assemblies

```
Vetolib.Billing.Contracts/   ← public : InvoiceDto, InvoiceItemDto, InvoiceStatus
Vetolib.Billing/             ← internal : Invoice entity, handlers, BillingDbContext
```

## Responsabilités

Facturation des prestations vétérinaires avec TVA UAE 5%, gestion du cycle de vie des factures.

---

## Entités Domain (internal)

### Invoice

```csharp
internal class Invoice : BaseEntity, IMultiTenant
{
    public Guid ClinicId { get; private set; }
    public Guid PatientId { get; private set; }
    public Guid? AppointmentId { get; private set; }   // optionnel
    public string InvoiceNumber { get; private set; }  // format: INV-{year}-{seq:0000}
    public InvoiceStatus Status { get; private set; }
    public DateTime IssuedAt { get; private set; }
    public DateTime DueAt { get; private set; }
    public string? TaxRegistrationNumber { get; private set; }  // TRN clinique (FTA UAE)
    public IReadOnlyList<InvoiceItem> Items { get; private set; }

    // Calculs — toujours depuis les items, jamais stockés en double
    public decimal SubtotalAed => Items.Sum(i => i.UnitPriceAed * i.Quantity);
    public decimal VatAmountAed => Math.Round(SubtotalAed * 0.05m, 2);
    public decimal TotalAed => SubtotalAed + VatAmountAed;

    public static Result<Invoice> Create(
        Guid clinicId, Guid patientId, Guid? appointmentId,
        string invoiceNumber, string? trn, DateTime dueAt) { ... }

    public Result AddItem(string description, decimal unitPrice, int quantity, string? serviceCode) { ... }
    public Result RemoveItem(Guid itemId) { ... }          // DRAFT seulement
    public Result Send() { ... }                           // DRAFT → Sent
    public Result MarkPaid(string paymentReference) { ... }// Sent → Paid
    public Result Cancel(string reason) { ... }            // Draft/Sent → Cancelled
    public Result MarkOverdue() { ... }                    // Sent → Overdue (appelé par job)
}
```

### InvoiceItem (Value Object)

```csharp
internal class InvoiceItem : BaseEntity
{
    public Guid InvoiceId { get; private set; }
    public string Description { get; private set; }
    public string? ServiceCode { get; private set; }    // code acte (ex: "CONSULT_GENERAL")
    public decimal UnitPriceAed { get; private set; }
    public int Quantity { get; private set; }
    // VatRate est toujours 5% en UAE — pas besoin de stocker par item
}
```

---

## Règles métier

- **Devise : AED uniquement**
- **TVA UAE : 5%** sur chaque facture (FTA compliance)
- Format numéro de facture : `INV-{année}-{séquence 4 chiffres}` auto-incrémenté par clinique
  - Ex : `INV-2025-0001`, `INV-2025-0042`
- La séquence repart à 1 chaque année civile
- Une facture `Paid` est **immuable** — aucune modification possible
- Seuls `Vet` et `Admin` peuvent créer/modifier une facture
- `Receptionist` : lecture uniquement
- Une facture peut exister sans appointment (actes directs, vente de produits)
- Le TRN (Tax Registration Number) de la clinique doit apparaître sur chaque facture imprimée
- Délai de paiement par défaut : 30 jours

### Statuts et transitions

```
Draft → Sent → Paid
Draft → Cancelled
Sent  → Overdue   (automatique quand DueAt est dépassé — job ou check au wake-up)
Sent  → Cancelled
Overdue → Paid    (paiement tardif autorisé)
```

### Création automatique depuis Agenda

Le module Billing écoute `AppointmentCompletedEvent` (depuis `Vetolib.Agenda.Contracts`)
et crée automatiquement une facture `Draft` avec un item "Consultation" par défaut.
Le `Vet` ou `Admin` peut ensuite enrichir les items (médicaments, actes supplémentaires).

---

## Endpoints

```
POST   /api/invoices                        → Result<InvoiceDto>                    (Vet, Admin)
GET    /api/invoices                        → Result<PagedResult<InvoiceDto>>       (filtre: status, from, to)
GET    /api/invoices/{id}                   → Result<InvoiceDto>
PUT    /api/invoices/{id}                   → Result<InvoiceDto>                    (Draft seulement)
POST   /api/invoices/{id}/items             → Result<InvoiceDto>                    (Draft seulement)
DELETE /api/invoices/{id}/items/{itemId}    → Result                               (Draft seulement)
PATCH  /api/invoices/{id}/send             → Result                               (Draft → Sent)
PATCH  /api/invoices/{id}/pay              → Result                               (body: { paymentReference })
PATCH  /api/invoices/{id}/cancel           → Result                               (body: { reason })
GET    /api/invoices/{id}/pdf              → PDF stream (pour impression)
```

---

## DTOs (Contracts, public)

```csharp
public record InvoiceDto(
    Guid Id,
    string InvoiceNumber,
    InvoiceStatus Status,
    Guid PatientId,
    string PatientName,
    Guid? AppointmentId,
    DateTime IssuedAt,
    DateTime DueAt,
    string? TaxRegistrationNumber,
    IReadOnlyList<InvoiceItemDto> Items,
    decimal SubtotalAed,
    decimal VatAmountAed,
    decimal TotalAed);

public record InvoiceItemDto(
    Guid Id,
    string Description,
    string? ServiceCode,
    decimal UnitPriceAed,
    int Quantity,
    decimal LineTotalAed);

public record AddInvoiceItemRequest(
    string Description,
    decimal UnitPriceAed,
    int Quantity,
    string? ServiceCode);
```

---

## Handlers attendus

| Command/Query | Retour | Déclencheur |
|---|---|---|
| `CreateInvoiceCommand` | `Result<InvoiceDto>` | Manuel ou `AppointmentCompletedEvent` |
| `AddInvoiceItemCommand` | `Result<InvoiceDto>` | Manuel |
| `RemoveInvoiceItemCommand` | `Result` | Manuel |
| `SendInvoiceCommand` | `Result` | Manuel |
| `PayInvoiceCommand` | `Result` | Manuel |
| `CancelInvoiceCommand` | `Result` | Manuel |
| `GetInvoicesQuery` | `Result<PagedResult<InvoiceDto>>` | - |
| `GetInvoiceByIdQuery` | `Result<InvoiceDto>` | - |

**Handler d'event :**

```csharp
internal class CreateDraftInvoiceOnAppointmentCompletedHandler
    : INotificationHandler<AppointmentCompletedEvent>
{
    public async Task Handle(AppointmentCompletedEvent notification, CancellationToken ct)
    {
        // Crée Invoice Draft avec 1 item "Consultation" au tarif par défaut de la clinique
    }
}
```

---

## Format facture imprimée (PDF) — conformité FTA

```
VETOLIB CLINIC — {Clinic Name}
TRN: {TaxRegistrationNumber}

INVOICE {InvoiceNumber}
Date: {IssuedAt}
Due:  {DueAt}

Patient: {PatientName}
Owner:   {OwnerName}

─────────────────────────────────────
Description         Qty    Unit    Total
─────────────────────────────────────
{item.Description}  {qty}  {unit}  {total} AED
─────────────────────────────────────
Subtotal (excl. VAT):         {SubtotalAed} AED
VAT 5%:                       {VatAmountAed} AED
TOTAL (incl. VAT):            {TotalAed} AED
─────────────────────────────────────
```

---

## Gherkins liés

`features/billing-and-records.feature` (section Billing)

---

## Dépendances

- `Vetolib.Shared.Kernel` + `Vetolib.Shared.Infrastructure`
- `Vetolib.Agenda.Contracts` : `AppointmentCompletedEvent` (Domain Event)
- `Vetolib.MedicalRecords.Contracts` : `PatientDto` (pour afficher le nom patient)
