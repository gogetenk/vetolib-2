# todo-front-dashboard-001.md — Frontend : Dashboard home

**Module** : Frontend / Dashboard
**Dépendances** : done-front-layout-001
[MSW: oui] — développement sans backend requis
[Branchement ultérieur] : crée todo-wire-dashboard-001 quand back-dashboard disponible
**Skills à lire** : `msw-mock-api`, `shadcn-nextjs`, `playwright-e2e`

---

## Périmètre exact

- `app/(dashboard)/page.tsx` — page home (actuellement redirige vers /appointments)
- `components/features/dashboard/StatsCards.tsx`
- `components/features/dashboard/TodayAppointments.tsx`
- `components/features/dashboard/RecentActivity.tsx`
- `lib/api/dashboard.ts`
- `src/mocks/handlers/dashboard.ts`
- `e2e/dashboard/dashboard.spec.ts`

---

## Layout

```
┌──────────────────────────────────────────────────────┐
│  Bonjour, Dr. Sarah 👋   Lundi 9 mars 2026           │
├──────────────┬──────────────┬───────────┬────────────┤
│ RDVs         │ En attente   │ Factures  │ Patients   │
│ aujourd'hui  │ check-in     │ impayées  │ total      │
│     8        │     3        │  AED 2,450│    127     │
├──────────────┴──────────────┴───────────┴────────────┤
│ Agenda du jour                    [Voir tout →]      │
│ 09:00  Max (Dog)    Dr. Sarah    CHECKED_IN          │
│ 10:30  Luna (Cat)   Dr. Ahmed    SCHEDULED           │
│ 11:00  Rocky (Dog)  Dr. Sarah    SCHEDULED           │
│ ...                                                  │
├──────────────────────────────────────────────────────┤
│ Activité récente                                     │
│ 14:32  Facture #042 payée — AED 350.00               │
│ 13:15  Dossier médical créé — Luna (Dr. Ahmed)       │
│ 11:00  RDV terminé — Max (Dr. Sarah)                 │
└──────────────────────────────────────────────────────┘
```

---

## Composants

### StatsCards

4 cards shadcn avec :
- RDVs aujourd'hui (count)
- En attente check-in (count, badge rouge si > 0)
- Factures impayées (montant AED)
- Patients total (count)

RBAC :
- RECEPTIONIST : voit RDVs + Factures, pas Patients total
- ASSISTANT : voit RDVs + Patients, pas Factures
- VET : voit RDVs filtrés sur lui-même + Patients

### TodayAppointments

Liste compacte des RDVs du jour, triée par heure.
Clic sur un RDV → navigate vers `/appointments/{id}`.
Bouton "Check In" inline pour les SCHEDULED (si rôle RECEPTIONIST ou ADMIN).

### RecentActivity

Feed des 10 dernières actions de la clinique (toutes catégories).
Icône selon le type : 📅 RDV, 💊 Dossier médical, 💰 Facture.

---

## MSW handlers

```typescript
// src/mocks/handlers/dashboard.ts
// GET /api/dashboard/stats → { appointmentsToday, pendingCheckin, unpaidInvoicesAed, totalPatients }
// GET /api/dashboard/today-appointments → AppointmentDto[]
// GET /api/dashboard/recent-activity → ActivityDto[]

const MOCK_STATS = {
  appointmentsToday: 8,
  pendingCheckin: 3,
  unpaidInvoicesAed: 2450.00,
  totalPatients: 127,
}
```

---

## Critère de complétion

```
□ Page home affiche les 4 stats cards
□ Agenda du jour visible avec statuts
□ Activité récente avec icônes par type
□ RBAC : stats filtrées selon le rôle
□ Clic RDV → navigate vers le détail
□ Tests Playwright verts
□ Renommer en done-front-dashboard-001.md
```
