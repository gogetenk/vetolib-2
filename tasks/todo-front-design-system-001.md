# todo-front-design-system-001.md — Harmoniser le design system

**Module** : Frontend
**Dépendances** : aucune
**Priorité** : MOYENNE
**Skills à lire** : `shadcn-nextjs`

---

## Objectif

S'assurer que le design system (shadcn/ui + Tailwind) est utilisé de manière cohérente partout. Pas de styles inline, pas de valeurs magiques, pas de composants custom quand shadcn en propose un.

## Checks

### 1. Composants shadcn
- Vérifier que tous les composants UI utilisent shadcn (pas de `<button>` natif, pas de `<input>` natif)
- Tables : utilisent `<Table>` shadcn (pas de `<table>` HTML)
- Dialogs : utilisent `<Dialog>` shadcn (pas de `modal` custom)
- Formulaires : utilisent les composants Form de shadcn avec react-hook-form

### 2. Tailwind
- Pas de valeurs magiques (`px-[13px]`) — utiliser les tokens Tailwind (`px-3`, `px-4`)
- Pas de `style={{}}` inline
- Dark mode ready (utilise les variables CSS de shadcn, pas de couleurs hardcodées)
- Responsive : utilise les breakpoints Tailwind (`sm:`, `md:`, `lg:`)

### 3. Tokens de couleur
- Utilise les variables shadcn (`text-primary`, `bg-muted`, `border-border`)
- Pas de couleurs hex directes (`text-[#ff0000]`)
- Palette cohérente entre toutes les pages

### 4. Typography
- Tailles cohérentes : h1, h2, h3, body, caption
- Font weight cohérent
- Line height adapté

### 5. Spacing
- Espacement cohérent entre sections (gap-6, gap-8)
- Padding des cards uniforme
- Margins entre éléments de formulaire

## Critère de complétion

```
□ Audit composants shadcn — tous utilisés correctement
□ Audit Tailwind — pas de valeurs magiques
□ Audit couleurs — palette cohérente
□ Audit typography — tailles uniformes
□ Tâches créées pour les fixes
□ Renommer en done
```
