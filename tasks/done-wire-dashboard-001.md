# todo-wire-dashboard-001.md — Wire : Dashboard front → back

**Dépendances** : done-back-dashboard-001, done-front-dashboard-001
**Skills** : `shadcn-nextjs`

---

## Objectif

Remplacer les handlers MSW du dashboard par les vrais appels à l'API backend.

## Actions

```
□ Supprimer src/mocks/handlers/dashboard.ts
□ Vérifier que le composant Dashboard appelle bien /api/dashboard/stats
□ Vérifier que GET /api/dashboard/today-appointments fonctionne
□ Vérifier que GET /api/dashboard/recent-activity fonctionne
□ Lancer Playwright E2E contre la vraie API (npm run test:e2e -- --grep dashboard)
□ Tous les tests verts
□ Renommer en done-wire-dashboard-001.md
```

## Vérifications spécifiques

- Stats reflètent les vraies données de la clinique connectée (isolation multi-tenant)
- `UnpaidInvoicesAed` en AED (pas EUR, pas USD)
- `AppointmentsToday` respecte le timezone Asia/Dubai (UTC+4)
- L'activité récente se rafraîchit si on crée un RDV dans un autre onglet (polling 60s)
