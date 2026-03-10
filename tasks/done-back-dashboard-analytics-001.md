# todo-back-dashboard-analytics-001.md — Dashboard analytics avancé

**Module** : Dashboard
**Dépendances** : aucune
**Priorité** : MOYENNE (post-MVP, rétention)
**Skills à lire** : `ardalis-result`, `aspnet-minimal-api`

---

## Objectif

Enrichir le dashboard avec des métriques avancées : revenus par mois, taux de no-show, répartition par espèce.

## Implémentation

### Backend

1. **GET /api/v1/dashboard/analytics** (VetOrAdmin)
   - `revenueByMonth` : array des 12 derniers mois `[{ month: "2026-01", total: 15000, currency: "AED" }]`
   - `noShowRate` : pourcentage de RDV CANCELLED ou non-COMPLETED sur les 30 derniers jours
   - `patientsBySpecies` : `[{ species: "Dog", count: 45 }, { species: "Cat", count: 30 }]`
   - `appointmentsByStatus` : répartition des statuts sur les 30 derniers jours
   - OutputCache : Dashboard1min policy

2. **Requêtes SQL optimisées**
   - GROUP BY mois sur les factures PAID
   - COUNT par species sur les patients
   - Tous filtrés par tenant (automatique)

### Frontend

3. **Section analytics sur le dashboard**
   - Graphe revenus par mois (bar chart) — utiliser recharts (déjà populaire avec Next.js)
   - Pie chart espèces
   - Stat card no-show rate
   - i18n EN + AR (labels des mois, "AED"/"درهم")

## Critère

```
□ GET /api/v1/dashboard/analytics retourne les 4 métriques
□ Graphe revenus + pie chart espèces sur le dashboard
□ No-show rate affiché
□ OutputCache 1min
□ i18n EN + AR
□ Renommer en done
```
