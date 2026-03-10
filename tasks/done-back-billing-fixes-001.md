# todo-back-billing-fixes-001.md — Fixes facturation (TRN + quantity)

**Module** : Billing
**Dépendances** : aucune
**Priorité** : BLOQUANT (PO review #2, #8)

---

## Contexte

1. Les factures n'affichent pas le TRN (Tax Registration Number) — obligatoire FTA UAE
2. Le backend ne stocke pas la quantité des lignes de facture (le frontend envoie `quantity` mais le backend l'ignore)

## Périmètre

### 1. TRN sur factures
- Ajouter `TaxRegistrationNumber` dans la configuration clinique (seed ou settings)
- Afficher sur le PDF (QuestPDF template) : "TRN: 100XXXXXXXXX"
- Afficher sur le frontend InvoiceDetail

### 2. Quantity sur InvoiceItem
- Vérifier que `InvoiceItem` domain entity stocke `Quantity`
- Vérifier que le calcul `Subtotal = Quantity × UnitPrice` est correct
- Migration si nécessaire

### 3. ClinicName dynamique
- Remplacer le hardcoded "Desert Paws" / "Happy Paws" dans les emails et factures
- Lire depuis la configuration ou un claim JWT `clinicName`

## Critère
```
□ TRN affiché sur le PDF facture
□ TRN configurable par clinique
□ Quantity stockée et utilisée dans le calcul
□ ClinicName dynamique partout
□ Tests unitaires mis à jour
□ Renommer en done
```
