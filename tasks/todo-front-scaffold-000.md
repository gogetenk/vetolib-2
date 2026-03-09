# todo-front-scaffold-000.md — Frontend : Scaffold Next.js 15

**Module** : Frontend
**Dépendances** : aucune (parallélisable avec todo-scaffold-000.md backend)
**Skills à lire** : `shadcn-nextjs`, `playwright-e2e`

---

## Objectif

Créer et configurer le projet `vetolib-frontend/` à la racine du repo.
Aucune feature — juste la structure qui compile et lance.

---

## Étapes

```bash
cd vetolib-frontend/  # créer à la racine du repo

npx create-next-app@latest . \
  --typescript \
  --tailwind \
  --eslint \
  --app \
  --src-dir \
  --import-alias "@/*"

# shadcn/ui
npx shadcn@latest init
# Choisir : New York style, zinc color, CSS variables yes

# Composants shadcn de base
npx shadcn@latest add button input label form card table badge
npx shadcn@latest add dialog sheet toast skeleton separator
npx shadcn@latest add navigation-menu sidebar

# Autres dépendances
npm install @tanstack/react-table react-hook-form zod @hookform/resolvers
npm install sonner date-fns
npm install -D @playwright/test @types/node
```

## Structure à créer

```
vetolib-frontend/
├── src/
│   ├── app/
│   │   ├── (auth)/
│   │   │   ├── login/
│   │   │   │   └── page.tsx       ← placeholder "Login page"
│   │   │   └── layout.tsx         ← layout minimaliste sans sidebar
│   │   ├── (dashboard)/
│   │   │   ├── appointments/
│   │   │   │   └── page.tsx       ← placeholder "Appointments"
│   │   │   ├── patients/
│   │   │   │   └── page.tsx       ← placeholder "Patients"
│   │   │   ├── medical-records/
│   │   │   │   └── page.tsx       ← placeholder "Medical Records"
│   │   │   ├── billing/
│   │   │   │   └── page.tsx       ← placeholder "Billing"
│   │   │   └── layout.tsx         ← layout avec sidebar + header
│   │   ├── layout.tsx
│   │   └── globals.css
│   ├── components/
│   │   └── features/              ← vide, rempli par les tâches suivantes
│   ├── lib/
│   │   ├── api/
│   │   │   └── client.ts          ← fetch helper avec JWT header + refresh
│   │   ├── auth.ts                ← getSession(), redirect si non connecté
│   │   └── utils.ts               ← cn() + formatAED() + formatDate(timezone)
│   └── middleware.ts               ← protection routes (dashboard)
├── e2e/
│   └── example.spec.ts            ← test vide de base
├── playwright.config.ts
└── next.config.ts
```

## Fichiers clés à implémenter

### `src/lib/api/client.ts`
```typescript
// Fetch helper qui :
// 1. Ajoute Authorization: Bearer {accessToken} depuis localStorage/cookie
// 2. Si 401 → appelle POST /api/auth/refresh automatiquement
// 3. Si refresh échoue → redirect /login
// 4. Retourne la réponse parsée ou lance une Error

export async function apiGet<T>(path: string): Promise<T>
export async function apiPost<T>(path: string, body: unknown): Promise<T>
export async function apiPatch<T>(path: string, body: unknown): Promise<T>
export async function apiDelete(path: string): Promise<void>
```

### `src/lib/utils.ts`
```typescript
import { clsx, type ClassValue } from "clsx"
import { twMerge } from "tailwind-merge"

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs))
}

// Formate un montant en AED (ex: "AED 1,250.00")
export function formatAED(amount: number): string

// Formate une date en tenant compte du timezone (défaut Asia/Dubai)
export function formatDate(date: string | Date, timezone?: string): string
```

### `src/middleware.ts`
```typescript
// Protège toutes les routes (dashboard) : redirige vers /login si pas de token
// Routes publiques : /login, /api/auth/*
```

### `playwright.config.ts`
```typescript
// baseURL: http://localhost:3000
// reporter: html
// trace: on-first-retry
// video: retain-on-failure (pour review PO)
// testDir: ./e2e
```

### `.env.local`
```
NEXT_PUBLIC_API_URL=http://localhost:5000
```

---

## Critère de complétion

```
□ npm run dev → localhost:3000 accessible
□ npm run build → 0 erreurs TypeScript
□ npm run lint → 0 erreurs
□ npx playwright test → 0 fails (test vide = skip)
□ Routes placeholder accessibles (login, appointments, patients...)
□ Renommer en done-front-scaffold-000.md
```
