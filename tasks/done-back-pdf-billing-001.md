# todo-back-pdf-billing-001.md — Génération PDF des factures

**Module** : Billing
**Dépendances** : done-back-migrations-001
**Skills à lire** : `ardalis-result`, `aspnet-minimal-api`

---

## Contexte

Le bouton "Download PDF" existe côté frontend mais l'endpoint backend n'existe pas. Une clinique vétérinaire doit pouvoir imprimer et envoyer des factures PDF à ses clients.

## Périmètre exact

### 1. Choisir une librairie PDF

Options recommandées pour .NET :
- **QuestPDF** (MIT, fluent API, moderne) ← recommandé
- iTextSharp (AGPL — problème de licence)
- PdfSharp (MIT, mais API bas niveau)

Installer QuestPDF dans le projet `Vetolib.Billing`.

### 2. Créer le template de facture

Le PDF doit contenir :

```
┌──────────────────────────────────────────┐
│ DESERT PAWS VETERINARY CLINIC            │
│ Dubai, UAE                                │
│ TRN: 100XXXXXXXXX (placeholder)          │
├──────────────────────────────────────────┤
│ INVOICE #INV-2026-042                     │
│ Date: 09/03/2026                          │
│ Status: PAID (09/03/2026)                 │
├──────────────────────────────────────────┤
│ Client: Ahmed Al-Mansoori                 │
│ Phone: +971 50 123 4567                   │
│ Patient: Max (Golden Retriever)           │
├──────────────────────────────────────────┤
│ # │ Description        │ Qty │ Price AED │
│ 1 │ Consultation       │  1  │   150.00  │
│ 2 │ Vaccination        │  1  │   200.00  │
├──────────────────────────────────────────┤
│                     Subtotal:   350.00 AED│
│                     VAT (5%):    17.50 AED│
│                     TOTAL:      367.50 AED│
├──────────────────────────────────────────┤
│ Thank you for trusting us with your pet!  │
└──────────────────────────────────────────┘
```

### 3. Endpoint API

```csharp
// GET /api/invoices/{id}/pdf → application/pdf
group.MapGet("/{id}/pdf", async (Guid id, ISender sender) =>
{
    var result = await sender.Send(new GenerateInvoicePdfQuery(id));
    return result.IsSuccess
        ? Results.File(result.Value, "application/pdf", $"invoice-{id}.pdf")
        : result.ToMinimalApiResult();
});
```

### 4. Query + Handler

```csharp
internal record GenerateInvoicePdfQuery(Guid InvoiceId) : IRequest<Result<byte[]>>;

internal class GenerateInvoicePdfHandler : IRequestHandler<GenerateInvoicePdfQuery, Result<byte[]>>
{
    // 1. Charger la facture avec ses items
    // 2. Vérifier qu'elle existe et est SENT ou PAID (pas DRAFT)
    // 3. Générer le PDF via QuestPDF
    // 4. Retourner byte[]
}
```

### 5. Frontend — brancher le bouton existant

Le `lib/api/billing.ts` a déjà `downloadInvoicePdf(id)` qui retourne un Blob. Vérifier que le bouton "Download PDF" dans `InvoiceDetail.tsx` appelle cette fonction et déclenche le téléchargement.

## Règles métier

- Seules les factures SENT ou PAID peuvent être téléchargées en PDF
- DRAFT → erreur 400 "Invoice must be sent before downloading"
- Le numéro de facture doit apparaître sur le PDF
- TVA 5% UAE (FTA) — toujours affichée même si 0
- Devise : AED uniquement

## Critère de complétion

```
□ QuestPDF installé et configuré
□ GET /api/invoices/{id}/pdf retourne un PDF valide
□ Template facture conforme (header clinique, lignes, TVA 5%, total AED)
□ DRAFT → 400, SENT/PAID → PDF
□ Bouton frontend déclenche le téléchargement
□ Test unitaire : vérifier que le handler retourne des bytes > 0
□ Renommer en done-back-pdf-billing-001.md
```
