# todo-front-billing-001.md — Frontend : Facturation

**Module** : Frontend / Billing
**Dépendances** : front-scaffold-000, back-billing-001
[MSW: oui] — développement sans backend requis
**Skills à lire** : `msw-mock-api`, `shadcn-nextjs`, `playwright-e2e`, `veterinary-domain`

---

## Périmètre exact

- `app/(dashboard)/billing/page.tsx` — liste des factures
- `app/(dashboard)/billing/new/page.tsx` — création facture
- `app/(dashboard)/billing/[id]/page.tsx` — détail + actions
- `components/features/billing/InvoiceTable.tsx`
- `components/features/billing/InvoiceForm.tsx`
- `components/features/billing/InvoiceDetail.tsx`
- `lib/api/billing.ts`
- `e2e/billing/billing.spec.ts`

---

## Liste des factures

Tableau avec colonnes :
| # Invoice | Patient | Date | Montant HT | TVA (5%) | Total AED | Statut | Actions |

Filtres : statut (DRAFT / SENT / PAID / CANCELLED), période (DatePicker range)
Total récapitulatif en bas : total des factures filtrées.

---

## Formulaire création facture

- Patient (Select avec recherche — autocomplete)
- Rendez-vous lié (Select optionnel)
- Lignes de facture (liste dynamique, bouton "+ Add item") :
  - Description (text)
  - Quantité (number)
  - Prix unitaire AED (number)
  - Sous-total calculé automatiquement
- Résumé : Sous-total HT, TVA 5%, **Total AED** (en gras)
- Notes (Textarea, optionnel)

---

## Détail facture + actions

Affichage propre type "facture" avec :
- Header : logo clinique (placeholder), numéro facture, date
- Client : nom propriétaire, téléphone
- Tableau des lignes
- Totaux : HT / TVA 5% / TTC en AED

Boutons selon statut :
| Statut | Actions disponibles |
|---|---|
| DRAFT | "Send" → SENT, "Edit", "Delete" |
| SENT | "Mark as Paid" → PAID, "Cancel" → CANCELLED |
| PAID | "Download PDF" (bouton uniquement, génération côté API) |
| CANCELLED | (aucun) |

---

## lib/api/billing.ts

```typescript
export async function getInvoices(filters?: InvoiceFilters): Promise<PagedResult<InvoiceDto>>
export async function getInvoice(id: string): Promise<InvoiceDto>
export async function createInvoice(data: CreateInvoiceRequest): Promise<InvoiceDto>
export async function sendInvoice(id: string): Promise<InvoiceDto>
export async function markAsPaid(id: string, paidAt?: string): Promise<InvoiceDto>
export async function cancelInvoice(id: string): Promise<InvoiceDto>
export async function downloadInvoicePdf(id: string): Promise<Blob>
```

---

## Règles d'affichage TVA UAE

- TVA = 5% sur le sous-total HT
- Toujours afficher les 3 lignes : Subtotal / VAT (5%) / Total
- Devise : AED (pas $, pas €) — utiliser `formatAED()` de `lib/utils.ts`
- Sur les factures PAID : afficher la date de paiement

---

## Critère de complétion

```
□ Liste des factures avec filtres
□ Création facture avec lignes dynamiques et calcul TVA
□ Transitions de statut fonctionnent
□ Affichage correct AED + TVA 5%
□ Tests Playwright passent
□ Renommer en done-front-billing-001.md
```
