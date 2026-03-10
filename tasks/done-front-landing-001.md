# todo-front-landing-001.md — Landing page : Hero + Social Proof + Features Grid

**Module** : Frontend
**Dépendances** : aucune
**Priorité** : HAUTE
**Skills à lire** : `shadcn-nextjs`, `playwright-e2e`
**Brief marketing** : `tasks/epic-landing-page.md` (sections 1-3)
[MSW: non]

---

## Objectif

Implémenter les 3 premières sections de la landing page marketing Vetolib : Hero, Social Proof Bar, Features Grid.

## Route

- `/en` → landing page EN (page par défaut, remplace le redirect actuel)
- `/ar` → landing page AR (RTL)

La landing page est une page publique (pas de auth required).

## Implémentation

### 1. Page route

`src/frontend/src/app/[locale]/page.tsx` — remplacer le contenu actuel par la landing page.

### 2. Section Hero

- Headline : "The Veterinary Practice Management Software Built for the UAE"
- Sous-titre (2 lignes max)
- CTA principal : "Start Free Trial" → lien vers `/[locale]/login` (pour l'instant)
- CTA secondaire : "Book a Demo" → mailto ou lien externe
- Visual : screenshot placeholder du dashboard (image statique, optimisée next/image)
- Above the fold sur desktop (1440px) et mobile (375px)

### 3. Section Social Proof Bar

- Barre horizontale avec 4 métriques (texte statique pour l'instant) :
  - "50+ clinics" / "5,000+ patients" / "AED 2M+ invoiced" / "99.9% uptime"
- Background subtle (gray-50 ou primary-50)

### 4. Section Features Grid

- Grille 3x2 (desktop) / 1 colonne (mobile) avec 6 features
- Chaque card : icône (lucide-react), titre, description (2 lignes)
- Features : Scheduling, Medical Records, Invoicing, Team Access, Dashboard, Notifications
- Contenu exact dans le brief `epic-landing-page.md` section 3

### 5. i18n

- Tous les textes via `next-intl` (messages EN + AR)
- RTL complet en AR
- `data-testid` sur chaque section et CTA

### 6. SEO

- `metadata` export avec title, description, openGraph
- H1 = headline du hero

## Critère

```
□ /en affiche la landing page avec Hero + Social Proof + Features
□ /ar affiche la version arabe RTL
□ CTA "Start Free Trial" → /[locale]/login
□ Mobile responsive (375px → 1440px)
□ next/image pour le visual hero
□ data-testid sur sections et CTAs
□ metadata SEO configurée
□ Renommer en done
```
