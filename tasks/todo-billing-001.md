# tasks/todo-billing-001.md
**Module** : Billing  
**Status** : [TODO]  
**Dépendances** : todo-auth-001.md  
**Gherkins** : features/billing-and-records.feature (section billing)

## Description
Implémenter la facturation complète avec TVA UAE.

Endpoints : `POST /api/v1/invoices`, `GET /api/v1/invoices`, `GET /api/v1/invoices/{id}`, `PATCH /api/v1/invoices/{id}/status`, `POST /api/v1/invoices/{id}/items`

Inclure : TVA 5% calculée automatiquement, numérotation séquentielle INV-{année}-{seq}, machine à états des statuts, immutabilité des factures PAID.
