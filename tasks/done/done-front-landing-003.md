# todo-front-landing-003.md — Landing page : FAQ + Final CTA + Footer + Analytics

**Module** : Frontend
**Dépendances** : done-front-landing-002
**Priorité** : HAUTE
**Skills à lire** : `shadcn-nextjs`
**Brief marketing** : `tasks/epic-landing-page.md` (sections 7-9 + analytics)
[MSW: non]

---

## Objectif

Sections finales de la landing page + analytics tracking + SEO structured data.

## Implémentation

### 1. Section FAQ

- Accordion (shadcn Accordion) avec 8 questions
- Contenu exact dans le brief section 7
- data-testid="faq-{index}" sur chaque item

### 2. Section Final CTA

- Background accent/primary
- Headline : "Ready to modernize your veterinary clinic?"
- Sous-titre + CTA "Start Free Trial" (bouton large)
- Reassurance text : "No credit card required. Setup in under 2 minutes."

### 3. Footer

- 4 colonnes : Product, Company, Resources, Legal
- Copyright + "Made for veterinary clinics in the UAE"
- Language switcher EN/AR
- Contact emails
- Le footer est un composant réutilisable (utilisable sur d'autres pages publiques)

### 4. Analytics Events

Créer un module `src/frontend/src/lib/analytics.ts` :

```typescript
export function trackEvent(name: string, properties?: Record<string, string>) {
  // Google Analytics 4 gtag or Plausible
  if (typeof window !== 'undefined' && window.gtag) {
    window.gtag('event', name, properties);
  }
}
```

Events à tracker (voir brief section 3) :
- cta_click_hero, cta_click_demo, cta_click_pricing, cta_click_final
- scroll_depth (25/50/75/100%)
- faq_expand (question index)
- language_switch
- pricing_toggle

### 5. SEO Structured Data

Dans le layout ou la page, ajouter le JSON-LD SoftwareApplication :
```json
{
  "@context": "https://schema.org",
  "@type": "SoftwareApplication",
  "name": "Vetolib",
  "applicationCategory": "BusinessApplication",
  "operatingSystem": "Web",
  "offers": {
    "@type": "AggregateOffer",
    "priceCurrency": "AED",
    "lowPrice": "249",
    "highPrice": "999"
  }
}
```

### 6. Playwright E2E

Créer `e2e/landing.spec.ts` :
- Vérifier que la page charge en EN
- Vérifier le switch vers AR (RTL)
- Vérifier que tous les CTA sont cliquables
- Vérifier l'accordion FAQ
- Vérifier le pricing toggle

## Critère

```
□ FAQ accordion 8 questions
□ Final CTA section
□ Footer 4 colonnes + language switch
□ analytics.ts module avec trackEvent
□ Events trackers sur tous les CTA
□ Scroll depth tracking
□ JSON-LD structured data
□ Playwright e2e/landing.spec.ts
□ i18n EN + AR complet
□ Renommer en done
```
