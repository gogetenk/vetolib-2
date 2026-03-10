# todo-front-posthog-consent-001 — Consent banner + toggle settings

**Module** : Frontend (Analytics)
**Dependances** : todo-front-posthog-setup-001
**Priorite** : HAUTE
**Skills a lire** : `shadcn-nextjs`
**[MSW: oui]** -- pas de dependance backend

---

## Contexte

PostHog est configure avec `opt_out_capturing_by_default: true` (tache setup). L'utilisateur doit pouvoir consentir au tracking via un banner, et changer son choix dans les settings.

Le PO (`docs/PO-POST-MVP-DECISIONS.md` section 2) precise :
- Analytics tracking est configurable par clinic (Admin opt-out)
- Privacy par defaut (opt-out by default, opt-in apres consentement)
- Banner simple accept/decline, pas de parametrage granulaire pour le MVP UAE

L'etude architecte (`docs/ANALYTICS-STUDY.md` section 3) precise :
- Stocker le choix dans `localStorage` (cle `analytics_consent`)
- PostHog gere nativement `opt_in_capturing()` et `opt_out_capturing()`
- Banner discret (bottom banner, pas de modal bloquant)

---

## Scope

### 1. ConsentBanner component

Creer `src/components/features/analytics/ConsentBanner.tsx` :

- `'use client'`
- Affiche un banner en bas de page (position fixed, bottom)
- Texte : "We use analytics to improve your experience. You can change this in Settings."
- Deux boutons : "Accept" et "Decline"
- i18n : textes dans `messages/en.json` et `messages/ar.json` sous la cle `analytics`
- Visible uniquement si `localStorage.getItem('analytics_consent')` est null (premier affichage)
- Au clic "Accept" :
  - `localStorage.setItem('analytics_consent', 'granted')`
  - `posthog.opt_in_capturing()`
  - Banner disparait
- Au clic "Decline" :
  - `localStorage.setItem('analytics_consent', 'denied')`
  - `posthog.opt_out_capturing()`
  - Banner disparait
- Design shadcn/ui : utiliser les composants Card ou Alert existants
- `data-testid="consent-banner"`, `data-testid="consent-accept"`, `data-testid="consent-decline"`

### 2. Integration dans le layout dashboard

Dans `src/app/[locale]/(dashboard)/layout.tsx` :
- Ajouter `<ConsentBanner />` apres le `<PostHogProvider>`
- Le banner ne s'affiche que si le choix n'a pas encore ete fait

### 3. Toggle dans settings/preferences

Ajouter une section "Privacy" dans la page settings utilisateur :

- Titre : "Analytics & Privacy"
- Description : "We collect anonymous usage data to improve the product. No personal information is tracked."
- Toggle switch (shadcn Switch component)
- Etat du toggle : lit `localStorage.getItem('analytics_consent')` au mount
  - `'granted'` -> ON
  - `'denied'` ou null -> OFF
- Au changement :
  - ON -> `posthog.opt_in_capturing()` + `localStorage.setItem('analytics_consent', 'granted')`
  - OFF -> `posthog.opt_out_capturing()` + `localStorage.setItem('analytics_consent', 'denied')`
- `data-testid="analytics-consent-toggle"`

### 4. Synchronisation PostHog au demarrage

Dans le `PostHogProvider.tsx` (cree dans la tache setup), au montage :
- Lire `localStorage.getItem('analytics_consent')`
- Si `'granted'` : appeler `posthog.opt_in_capturing()` (restaure le consentement entre sessions)
- Si `'denied'` ou null : ne rien faire (PostHog est deja opt-out par defaut)

### 5. Traductions i18n

Ajouter dans `messages/en.json` :
```json
{
  "analytics": {
    "consent_banner": {
      "message": "We use analytics to improve your experience. You can change this in Settings.",
      "accept": "Accept",
      "decline": "Decline"
    },
    "settings": {
      "title": "Analytics & Privacy",
      "description": "We collect anonymous usage data to improve the product. No personal information is tracked.",
      "toggle_label": "Allow anonymous usage analytics"
    }
  }
}
```

Ajouter dans `messages/ar.json` :
```json
{
  "analytics": {
    "consent_banner": {
      "message": "نستخدم التحليلات لتحسين تجربتك. يمكنك تغيير ذلك في الإعدادات.",
      "accept": "قبول",
      "decline": "رفض"
    },
    "settings": {
      "title": "التحليلات والخصوصية",
      "description": "نجمع بيانات استخدام مجهولة لتحسين المنتج. لا يتم تتبع أي معلومات شخصية.",
      "toggle_label": "السماح بتحليلات الاستخدام المجهولة"
    }
  }
}
```

### 6. Tests Playwright

Creer `e2e/analytics/consent.spec.ts` :

```typescript
test('consent banner appears on first visit', async ({ page }) => {
  await page.goto('/en/appointments')
  await expect(page.getByTestId('consent-banner')).toBeVisible()
})

test('consent banner disappears after accepting', async ({ page }) => {
  await page.goto('/en/appointments')
  await page.getByTestId('consent-accept').click()
  await expect(page.getByTestId('consent-banner')).not.toBeVisible()
})

test('consent banner disappears after declining', async ({ page }) => {
  await page.goto('/en/appointments')
  await page.getByTestId('consent-decline').click()
  await expect(page.getByTestId('consent-banner')).not.toBeVisible()
})

test('consent banner does not reappear after choice', async ({ page }) => {
  await page.goto('/en/appointments')
  await page.getByTestId('consent-accept').click()
  await page.reload()
  await expect(page.getByTestId('consent-banner')).not.toBeVisible()
})

test('analytics toggle in settings reflects consent state', async ({ page }) => {
  // Accept analytics via banner, then check settings toggle is ON
})
```

---

## Ce qui NE change PAS

- Backend : aucun module modifie
- MSW handlers : pas modifies
- Landing page : pas de banner de consentement (gtag uniquement)

---

## Critere de completion

```
[] ConsentBanner.tsx cree avec accept/decline
[] Banner integre dans (dashboard)/layout.tsx
[] Banner visible uniquement au premier affichage (localStorage check)
[] Accept -> posthog.opt_in_capturing() + localStorage 'granted'
[] Decline -> posthog.opt_out_capturing() + localStorage 'denied'
[] Section "Analytics & Privacy" ajoutee dans settings
[] Toggle switch fonctionnel (sync avec posthog opt_in/opt_out)
[] Traductions EN + AR ajoutees dans messages/*.json
[] data-testid presents sur banner, boutons, toggle
[] PostHogProvider restaure le consentement au demarrage (localStorage)
[] Tests Playwright consent verts
[] Design RTL correct pour le banner en arabe
[] npm run build → 0 erreur
[] Renommer en done-front-posthog-consent-001.md
```
