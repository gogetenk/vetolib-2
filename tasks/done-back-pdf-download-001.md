# todo-back-pdf-download-001.md — Endpoint téléchargement PDF factures

**Module** : Billing
**Dépendances** : aucune
**Priorité** : HAUTE (post-MVP, rétention)
**Skills à lire** : `ardalis-result`, `aspnet-minimal-api`

---

## Objectif

Exposer un endpoint de téléchargement PDF pour les factures. QuestPDF est déjà intégré pour la génération.

## Implémentation

### Backend

1. **GET /api/v1/invoices/{id}/pdf** (ClinicStaff)
   - Génère le PDF via QuestPDF (déjà câblé dans le module Billing)
   - Retourne `application/pdf` avec Content-Disposition: attachment
   - Filename : `INV-{number}.pdf`
   - OutputCache : cache 5min par invoiceId (le PDF ne change pas souvent)

2. **Vérifications**
   - Facture doit exister (404 sinon)
   - Multi-tenant filter (automatique)
   - Facture doit être en SENT ou PAID (pas DRAFT)

### Frontend

3. **Bouton "Download PDF" sur la page facture**
   - Icône download sur chaque ligne du tableau des factures (si statut ≠ DRAFT)
   - Ouvre le PDF dans un nouvel onglet ou télécharge directement
   - i18n EN + AR

## Critère

```
□ GET /api/v1/invoices/{id}/pdf retourne un PDF valide
□ Content-Disposition: attachment; filename="INV-2026-001.pdf"
□ 404 si facture inexistante, 400 si DRAFT
□ Bouton download dans le frontend
□ OutputCache 5min
□ Test BDD : download OK, DRAFT refusé
□ Renommer en done
```
