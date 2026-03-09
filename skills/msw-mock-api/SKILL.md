# Skill: MSW — Mock Service Worker (développement frontend découplé)

## Principe

MSW intercepte les requêtes HTTP dans le navigateur (Service Worker) et en Node.js (tests).
L'UI est développée **entièrement** avec des données mockées. La même `lib/api/` est utilisée
en dev (MSW intercepte) et en prod (vrai backend). Zéro code conditionnel dans les composants.

## Installation

```bash
npm install msw --save-dev
npx msw init public/ --save
```

## Structure des mocks

```
vetolib-frontend/src/mocks/
├── browser.ts          ← setup MSW pour le navigateur (next dev)
├── node.ts             ← setup MSW pour Node.js (Playwright, tests unitaires)
└── handlers/
    ├── index.ts        ← exporte tous les handlers
    ├── auth.ts
    ├── appointments.ts
    ├── patients.ts
    └── billing.ts
```

## Pattern d'un handler

```typescript
// src/mocks/handlers/appointments.ts
import { http, HttpResponse, delay } from 'msw'
import type { AppointmentDto, PagedResult } from '@/lib/api/appointments'

const MOCK_APPOINTMENTS: AppointmentDto[] = [
  {
    id: 'a1b2c3d4-0000-0000-0000-000000000001',
    patientName: 'Max',
    species: 'Dog',
    breed: 'Golden Retriever',
    ownerName: 'Ahmed Al-Rashid',
    ownerPhone: '+971 50 123 4567',
    vetName: 'Dr. Sarah Johnson',
    status: 'SCHEDULED',
    scheduledAt: new Date(Date.now() + 3600_000).toISOString(),
    reason: 'Annual vaccination',
    clinicId: 'clinic-001',
  },
  {
    id: 'a1b2c3d4-0000-0000-0000-000000000002',
    patientName: 'Luna',
    species: 'Cat',
    breed: 'Siamese',
    ownerName: 'Fatima Hassan',
    ownerPhone: '+971 55 987 6543',
    vetName: 'Dr. Ahmed Khalil',
    status: 'IN_PROGRESS',
    scheduledAt: new Date(Date.now() - 1800_000).toISOString(),
    reason: 'Skin condition follow-up',
    clinicId: 'clinic-001',
  },
]

export const appointmentHandlers = [
  // GET /api/appointments (avec pagination)
  http.get('/api/appointments', ({ request }) => {
    const url = new URL(request.url)
    const status = url.searchParams.get('status')
    const items = status
      ? MOCK_APPOINTMENTS.filter(a => a.status === status)
      : MOCK_APPOINTMENTS

    return HttpResponse.json<PagedResult<AppointmentDto>>({
      items,
      totalCount: items.length,
      page: 1,
      pageSize: 10,
    })
  }),

  // GET /api/appointments/:id
  http.get('/api/appointments/:id', ({ params }) => {
    const appt = MOCK_APPOINTMENTS.find(a => a.id === params.id)
    if (!appt) return new HttpResponse(null, { status: 404 })
    return HttpResponse.json(appt)
  }),

  // POST /api/appointments
  http.post('/api/appointments', async ({ request }) => {
    const body = await request.json() as Partial<AppointmentDto>
    const newAppt: AppointmentDto = {
      id: crypto.randomUUID(),
      status: 'SCHEDULED',
      clinicId: 'clinic-001',
      ...body,
    } as AppointmentDto
    MOCK_APPOINTMENTS.push(newAppt)
    return HttpResponse.json(newAppt, { status: 201 })
  }),

  // PATCH /api/appointments/:id/transition
  http.patch('/api/appointments/:id/transition', async ({ params, request }) => {
    const body = await request.json() as { action: string }
    const appt = MOCK_APPOINTMENTS.find(a => a.id === params.id)
    if (!appt) return new HttpResponse(null, { status: 404 })

    const transitions: Record<string, string> = {
      CHECK_IN: 'CHECKED_IN',
      START: 'IN_PROGRESS',
      COMPLETE: 'COMPLETED',
      CANCEL: 'CANCELLED',
    }
    appt.status = transitions[body.action] ?? appt.status
    return HttpResponse.json(appt)
  }),
]
```

## Setup browser (next dev)

```typescript
// src/mocks/browser.ts
import { setupWorker } from 'msw/browser'
import { handlers } from './handlers'

export const worker = setupWorker(...handlers)
```

```typescript
// src/mocks/handlers/index.ts
import { authHandlers } from './auth'
import { appointmentHandlers } from './appointments'
import { patientHandlers } from './patients'
import { billingHandlers } from './billing'

export const handlers = [
  ...authHandlers,
  ...appointmentHandlers,
  ...patientHandlers,
  ...billingHandlers,
]
```

```typescript
// src/app/layout.tsx — activer uniquement en développement
export default async function RootLayout({ children }) {
  // Côté serveur : MSW ne s'active pas (Next.js SSR)
  // L'activation se fait dans un Client Component wrapper
  return (
    <html lang="en">
      <body>
        <MSWProvider>{children}</MSWProvider>
      </body>
    </html>
  )
}
```

```typescript
// src/components/MSWProvider.tsx
'use client'
import { useEffect, useState } from 'react'

export function MSWProvider({ children }: { children: React.ReactNode }) {
  const [ready, setReady] = useState(process.env.NODE_ENV !== 'development')

  useEffect(() => {
    if (process.env.NODE_ENV === 'development') {
      import('@/mocks/browser').then(({ worker }) =>
        worker.start({ onUnhandledRequest: 'bypass' })
      ).then(() => setReady(true))
    }
  }, [])

  if (!ready) return null
  return <>{children}</>
}
```

## Setup Node.js (Playwright)

```typescript
// vetolib-frontend/e2e/setup/msw.setup.ts
import { setupServer } from 'msw/node'
import { handlers } from '../../src/mocks/handlers'

export const server = setupServer(...handlers)

// Dans playwright.config.ts : utiliser next dev (qui active MSW via browser)
// Les tests Playwright s'exécutent contre l'app Next.js qui utilise MSW
// PAS besoin de setup server Node.js pour Playwright — MSW browser suffit
```

## Règles importantes

1. **Les mocks doivent être réalistes** : utiliser de vraies données UAE (noms arabes, AED, timezone Dubai)
2. **Les IDs doivent être des UUID valides** — Playwright va les utiliser dans les URLs
3. **Mutation des mocks en mémoire** : `MOCK_APPOINTMENTS.push(newAppt)` pour simuler la persistance dans la session
4. **Un handler par module** dans `handlers/` — jamais un gros fichier handlers.ts
5. **Ne jamais importer les handlers en production** — le tree-shaking + condition `NODE_ENV` s'en charge

## Branchement (tâche wire)

Quand le backend est prêt, la tâche `wire-{module}` :
1. Supprime `src/mocks/handlers/{module}.ts`
2. Retire le module de `src/mocks/handlers/index.ts`
3. Lance Playwright contre l'API réelle (backend Aspire doit tourner)
4. Les tests doivent rester verts — sinon le contrat diverge → question au PO

## Vérification rapide

```bash
# L'app tourne avec MSW
npm run dev
# Ouvrir DevTools → Network → voir les requêtes interceptées par [MSW]

# Tests Playwright contre MSW
npx playwright test
# Tous verts → UI complète et fonctionnelle, indépendamment du backend
```
