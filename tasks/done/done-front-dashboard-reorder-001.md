# todo-front-dashboard-reorder-001 -- Reorder dashboard + cap stats cards

**Module** : Frontend
**Priority** : Haute (UX audit P1)
**Dependencies** : none

## Context

UX audit identified:
- D1: Dashboard shows 6 sections, TodayAppointments should be first
- D2: StatsCards should be capped at 4 (currently shows all)
- D3: "AED 0" today revenue is hardcoded/fake -- disable or wire real data

## Scope

File: `src/frontend/src/app/[locale]/(dashboard)/dashboard/page.tsx`

## Rules

- Reorder: TodayAppointments first, then StatsCards (max 4), then HealthAlerts, then others
- If revenue stat is hardcoded "AED 0", hide it or show "No data" placeholder
- Keep all `data-testid` attributes
- `npm run lint` + `npm run build` GREEN

## Completion criteria

- [ ] TodayAppointments is the first section on dashboard
- [ ] StatsCards capped at 4 visible cards
- [ ] No fake "AED 0" revenue displayed
- [ ] `npm run lint` GREEN
- [ ] `npm run build` GREEN
