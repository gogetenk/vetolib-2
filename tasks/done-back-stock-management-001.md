# todo-back-stock-management-001.md — Gestion de stock médicaments/vaccins

**Module** : Nouveau module Stock (ou sous-module MedicalRecords)
**Dépendances** : aucune
**Priorité** : MOYENNE (post-MVP, rétention)
**Skills à lire** : `ardalis-result`, `cqrs-mediatr`, `ardalis-modular-monolith`, `multitenant-efcore`

---

## Objectif

Permettre aux cliniques de gérer leur inventaire de médicaments et vaccins avec alertes de seuil bas.

## Implémentation

### Backend — Nouveau module Stock

1. **Domain**
   - `StockItem { Id, ClinicId, Name, Category (Medication|Vaccine|Supply), Quantity, Unit, MinThreshold, ExpiryDate, CreatedAt, UpdatedAt }`
   - `StockMovement { Id, StockItemId, MovementType (IN|OUT|ADJUSTMENT), Quantity, Reason, CreatedBy, CreatedAt }`

2. **Endpoints**
   - `GET /api/v1/stock` — liste avec filtres (category, low-stock, expiring-soon)
   - `POST /api/v1/stock` — créer un item (VetOrAdmin)
   - `PATCH /api/v1/stock/{id}` — modifier seuil, nom
   - `POST /api/v1/stock/{id}/movements` — enregistrer entrée/sortie
   - `GET /api/v1/stock/alerts` — items sous le seuil + expirant dans 30 jours

3. **Events**
   - `StockLowEvent` → notification email quand qty < MinThreshold
   - `StockExpiringEvent` → notification 30 jours avant expiry

### Frontend

4. **Page /[locale]/stock**
   - Tableau avec colonnes : nom, catégorie, qté, seuil, expiry, statut
   - Badge rouge si low-stock, orange si expiring
   - Formulaire ajout/mouvement
   - i18n EN + AR

## Architecture

Suivre le pattern 2 assemblies :
- `Vetolib.Stock.Contracts/` — DTOs, events
- `Vetolib.Stock/` — handlers, domain, infra

## Critère

```
□ Module Stock créé (2 assemblies)
□ CRUD stock items + mouvements
□ Alertes low-stock et expiry
□ Page frontend /stock
□ Events vers Notifications
□ Tests BDD
□ i18n EN + AR
□ Renommer en done
```
