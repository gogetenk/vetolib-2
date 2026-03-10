# todo-front-landing-002.md — Landing page : How It Works + Pricing + Testimonials

**Module** : Frontend
**Dépendances** : done-front-landing-001
**Priorité** : HAUTE
**Skills à lire** : `shadcn-nextjs`
**Brief marketing** : `tasks/epic-landing-page.md` (sections 4-6)
[MSW: non]

---

## Objectif

Sections 4-6 de la landing page : How It Works, Pricing, Testimonials.

## Implémentation

### 1. Section How It Works

- 3 étapes numérotées avec icône + titre + description
- Layout horizontal (desktop) / vertical (mobile)
- 1: Sign Up → 2: Invite Team → 3: Start Managing
- CTA : "Get Started Free" → /[locale]/login

### 2. Section Pricing

- 3 colonnes : Starter (299 AED), Pro (599 AED, badge "Most Popular"), Enterprise (sur devis)
- Toggle mensuel/annuel (249/499/999 AED annuel)
- Chaque plan : liste de features avec checkmarks
- CTA par plan : "Start Free Trial" / "Start Free Trial" / "Contact Sales"
- Note : "All plans include 14-day free trial. No credit card required."
- Prix en AED, mention "excl. VAT 5%"

### 3. Section Testimonials

- 3 cards avec quote, nom, titre, clinique
- Textes du brief (marqués fictifs — data-testid="testimonial-placeholder" pour rappeler)
- Layout : carousel mobile, 3 colonnes desktop

### 4. i18n

- Tous les textes EN + AR
- Pricing : "AED" en EN, "درهم" en AR
- data-testid sur chaque section

## Critère

```
□ How It Works 3 étapes visibles
□ Pricing 3 plans avec toggle mensuel/annuel
□ Testimonials 3 cards
□ i18n EN + AR complet
□ Mobile responsive
□ data-testid sur sections
□ Renommer en done
```
