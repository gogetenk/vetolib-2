# todo-front-auth-001.md — Frontend : Page Login

**Module** : Frontend / Auth
**Dépendances** : front-scaffold-000
[Branchement ultérieur] : back-auth-001 → crée automatiquement todo-wire-auth-001 (API login doit exister)
**Gherkins** : `features/auth/login.feature` (scénarios UI uniquement)
[MSW: oui] — développement sans backend requis
**Skills à lire** : `msw-mock-api`, `shadcn-nextjs`, `playwright-e2e`

---

## Périmètre exact

Implémenter dans `vetolib-frontend/src/` :

- `app/(auth)/login/page.tsx` — Server Component wrapper
- `components/features/auth/LoginForm.tsx` — Client Component avec formulaire
- `lib/api/auth.ts` — fonctions login() + refresh()
- `e2e/auth/login.spec.ts` — Tests Playwright

---

## UI à implémenter

```
┌─────────────────────────────────┐
│         Vetolib                 │
│    Veterinary Management        │
│                                 │
│  Email                          │
│  [________________________]     │
│                                 │
│  Password                       │
│  [________________________]     │
│                                 │
│  [    Sign In    ]              │
│                                 │
│  ⚠ (message erreur ici)         │
└─────────────────────────────────┘
```

- Centré verticalement sur la page
- Card shadcn/ui
- Logo/titre en haut
- Formulaire avec react-hook-form + zod
- Bouton désactivé pendant le chargement (Skeleton ou spinner)
- Message d'erreur visible sous le bouton (`data-testid="error-message"`)
- Pas de "Forgot password" (hors MVP)

---

## Composant LoginForm.tsx

```typescript
"use client"
// Schéma zod : email (email valid), password (min 1)
// onSubmit : appelle lib/api/auth.ts login()
// Succès : stocke tokens, redirect vers /appointments
// Erreur "INVALID_CREDENTIALS" → "Invalid email or password"
// Erreur "ACCOUNT_LOCKED" → "Account locked. Try again in 15 minutes."
// Erreur réseau → "Connection error. Please try again."
// data-testid obligatoires :
//   "email-input", "password-input", "signin-button", "error-message"
```

---

## lib/api/auth.ts

```typescript
export interface AuthTokens {
  accessToken: string
  refreshToken: string
  expiresIn: number
}

// POST /api/auth/login
export async function login(email: string, password: string): Promise<AuthTokens>

// POST /api/auth/refresh
export async function refreshTokens(refreshToken: string): Promise<AuthTokens>

// Stockage tokens : httpOnly cookie via /api/auth/session (route Next.js)
// OU localStorage — à décider lors de l'implémentation, noter dans decisions.md
```

---

## Tests Playwright `e2e/auth/login.spec.ts`

```typescript
// Scénarios à couvrir (extraits de features/auth/login.feature) :
// - Login valide → redirect /appointments
// - Email invalide → message erreur
// - Mauvais mot de passe → "Invalid email or password"
// - 5 tentatives → "Account locked. Try again in 15 minutes."
// - Champs vides → validation côté client (pas d'appel API)
// Page Object : LoginPage avec les méthodes goto(), fillEmail(), fillPassword(), submit(), getError()
```

---

## Critère de complétion

```
□ Tous les scénarios Playwright passent
□ npm run build → 0 erreurs
□ Redirect /login → /appointments si déjà connecté
□ Renommer en done-front-auth-001.md
```
