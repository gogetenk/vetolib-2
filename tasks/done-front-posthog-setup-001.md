# todo-front-posthog-setup-001 — PostHog Provider + configuration

**Module** : Frontend (Analytics)
**Dependances** : aucune
**Priorite** : HAUTE
**Skills a lire** : `shadcn-nextjs`
**[MSW: oui]** -- pas de dependance backend

---

## Contexte

L'etude `docs/ANALYTICS-STUDY.md` recommande PostHog Cloud (region EU) cote frontend uniquement pour l'analytics produit. Le PO (`docs/PO-POST-MVP-DECISIONS.md`) confirme la priorite HAUTE.

Le frontend dispose actuellement d'un `lib/analytics.ts` qui utilise `window.gtag` (GA4) pour la landing page uniquement. Aucun tracking dans l'application SaaS (dashboard, patients, agenda, billing).

**Divergence PO/Architecte** : le PO mentionne "server-side SDK" mais l'architecte recommande `posthog-js` cote client. Les events a tracker sont des interactions UI (clics, navigation, funnels) qui ne peuvent pas etre captures server-side. On suit la recommandation architecte : `posthog-js` cote client.

---

## Scope

### 1. Installation

```bash
npm install posthog-js
```

### 2. Configuration PostHog

Creer `src/lib/posthog.ts` :
- Lire `NEXT_PUBLIC_POSTHOG_KEY` depuis les variables d'environnement
- Lire `NEXT_PUBLIC_POSTHOG_HOST` (defaut : `https://eu.i.posthog.com`)
- Options :
  - `api_host`: valeur de NEXT_PUBLIC_POSTHOG_HOST
  - `capture_pageview`: false (gere manuellement via usePathname)
  - `capture_pageleave`: true
  - `persistence`: `'localStorage'`
  - `opt_out_capturing_by_default`: true (RGPD/privacy by default)
- Si `NEXT_PUBLIC_POSTHOG_KEY` est vide ou undefined, PostHog ne s'initialise pas (safe pour les tests Playwright et le dev sans cle)

### 3. PostHogProvider

Creer `src/components/PostHogProvider.tsx` :
- `'use client'`
- Initialiser `posthog-js` avec la config de `lib/posthog.ts`
- Hook `usePathname()` + `useSearchParams()` + `useEffect` pour envoyer `$pageview` a chaque changement de route (pattern standard Next.js App Router)
- Ne PAS initialiser si la cle est absente
- Exposer le provider React de `posthog-js/react`

### 4. Integration dans les layouts

Modifier `src/app/[locale]/(dashboard)/layout.tsx` :
- Ajouter `<PostHogProvider>` autour du contenu

Modifier `src/app/[locale]/(auth)/layout.tsx` :
- Ajouter `<PostHogProvider>` autour du contenu

NE PAS toucher :
- `src/app/layout.tsx` (Server Component, pas de PostHog)
- La landing page (conserve gtag pour le marketing Google Ads)

### 5. Identification utilisateur

Dans le flow de login (`LoginForm.tsx` ou `lib/auth.ts` apres login reussi) :
```typescript
posthog.identify(user.id, {
  role: user.role,
  clinic_id: user.clinicId,
})
```

NE PAS envoyer de PII (email, nom) a PostHog. Uniquement l'ID, le role, et le clinicId.

Au logout :
```typescript
posthog.reset()
```

### 6. Groupement multi-tenant

Apres identification, grouper par clinique :
```typescript
posthog.group('clinic', user.clinicId)
```

Cela permet le filtrage par clinique dans les dashboards PostHog.

### 7. Refactoriser lib/analytics.ts

Remplacer l'implementation gtag par PostHog dans `lib/analytics.ts` :
- Garder la meme API publique (`trackEvent`, `AnalyticsEvents`)
- `trackEvent()` appelle `posthog.capture()` au lieu de `window.gtag()`
- Conserver les events landing page existants dans `AnalyticsEvents` (CTA_HERO, etc.)
- La landing page continue d'utiliser gtag directement (pas de regression)
- Ajouter les nouveaux noms d'events (voir tache todo-front-posthog-events-001)

### 8. Variables d'environnement

Ajouter dans `.env.example` :
```
NEXT_PUBLIC_POSTHOG_KEY=
NEXT_PUBLIC_POSTHOG_HOST=https://eu.i.posthog.com
```

---

## Ce qui NE change PAS

- Backend : aucun module modifie
- Landing page : conserve gtag
- MSW handlers : pas modifies (PostHog n'utilise pas les memes endpoints)
- Shared/ : GELE
- AppHost, Vetolib.Api : GELES

---

## Regles

- Zero code conditionnel `if (process.env.NODE_ENV === 'development')` dans les composants
- PostHog desactive si la cle est absente (env de test, dev sans cle)
- Aucun PII envoye a PostHog (pas d'email, pas de nom)
- `opt_out_capturing_by_default: true` -- l'utilisateur doit consentir avant tout tracking

---

## Critere de completion

```
[] posthog-js installe dans package.json
[] src/lib/posthog.ts cree avec config (cle env, opt_out par defaut)
[] src/components/PostHogProvider.tsx cree ('use client', pageview tracking)
[] PostHogProvider integre dans (dashboard)/layout.tsx
[] PostHogProvider integre dans (auth)/layout.tsx
[] Landing page NON modifiee (gtag conserve)
[] posthog.identify() appele au login (id, role, clinicId — pas de PII)
[] posthog.group('clinic', clinicId) appele au login
[] posthog.reset() appele au logout
[] lib/analytics.ts refactorise : trackEvent() utilise posthog.capture()
[] .env.example mis a jour avec NEXT_PUBLIC_POSTHOG_KEY et NEXT_PUBLIC_POSTHOG_HOST
[] PostHog ne s'initialise pas si la cle est vide (safe pour tests)
[] npm run build → 0 erreur
[] Renommer en done-front-posthog-setup-001.md
```
