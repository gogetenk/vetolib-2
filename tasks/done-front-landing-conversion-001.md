# todo-front-landing-conversion-001.md — Optimiser la landing page pour la conversion

**Module** : Frontend
**Dépendances** : aucune
**Priorité** : HAUTE
**Skills à lire** : `shadcn-nextjs`

---

## Contexte

La landing page est la première impression du produit. Elle doit convertir les visiteurs (vétérinaires UAE) en utilisateurs. Chaque élément doit être optimisé pour maximiser le taux de signup.

## Audit et optimisations

### 1. Hero section
- Headline : est-elle claire, spécifique, orientée bénéfice ? (pas "Welcome to Vetolib" mais "Manage your veterinary clinic in minutes")
- Sous-titre : proposition de valeur en 1 phrase
- CTA principal : visible, contrasté, texte action ("Start free trial" > "Sign up")
- Image/illustration : pertinente, professionnelle, pas de stock photo générique
- Social proof near CTA (ex: "Trusted by 50+ clinics in UAE")

### 2. Value proposition
- 3-4 features max, pas 10
- Chaque feature : icône + titre court + description 1 ligne
- Orienté bénéfice utilisateur (pas "Multi-tenant architecture" mais "Manage multiple clinics from one account")
- Screenshots/mockups du produit réel

### 3. Social proof
- Témoignages (même fictifs pour le MVP, mais réalistes UAE)
- Logos de cliniques (si disponibles)
- Chiffres ("Save 2 hours/day on admin tasks")

### 4. Pricing / CTA final
- Section pricing claire si applicable
- CTA de conversion en bas de page (répétition)
- "No credit card required" ou "Free for first clinic"

### 5. Technique
- Performance Lighthouse > 90
- SEO : meta tags, OG tags, structured data
- Mobile-first : la landing doit être parfaite sur mobile
- Animations subtiles (scroll reveal, hover effects) — pas de surcharge
- Temps de chargement < 2s

### 6. Copywriting
- Tout en anglais (marché UAE)
- Ton professionnel mais accessible
- Éviter le jargon technique
- Urgence douce ("Limited beta access" ou "Join the waitlist")

## Étapes

1. Lire la landing page actuelle (`src/frontend/src/components/features/landing/`)
2. Lire les pages de signup (`src/frontend/src/app/[locale]/`)
3. Analyser chaque section contre les critères ci-dessus
4. Proposer et implémenter les améliorations
5. Vérifier le responsive (mobile, tablet, desktop)

## Critère de complétion

```
□ Hero section optimisée (headline, CTA, visual)
□ Features section claire et orientée bénéfice
□ Social proof ajoutée
□ CTA final en bas de page
□ Performance Lighthouse > 90
□ Mobile responsive vérifié
□ Renommer en done
```
