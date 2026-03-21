# todo-front-ux-audit-001.md — Audit UX complet du dashboard

**Module** : Frontend
**Dépendances** : aucune
**Priorité** : HAUTE
**Skills à lire** : `shadcn-nextjs`

---

## Objectif

Audit UX complet de toutes les pages dashboard pour identifier les frictions, incohérences visuelles, et améliorations d'ergonomie.

## Pages à auditer

1. **Dashboard** (`/[locale]/dashboard`) — KPIs, graphiques, actions rapides
2. **Patients** (`/[locale]/patients`) — liste, recherche, fiche détail
3. **Agenda** (`/[locale]/appointments`) — vue liste, création, détail
4. **Billing** (`/[locale]/billing`) — factures, détail, PDF
5. **Settings** (`/[locale]/settings/team`) — gestion équipe
6. **Sidebar + Header** — navigation, menu utilisateur

## Axes d'audit

### 1. Cohérence visuelle
- Spacing uniformes (padding, margins, gaps)
- Tailles de police cohérentes (titres, sous-titres, body)
- Couleurs : palette respectée partout (primary, secondary, muted, destructive)
- Icônes : style cohérent (Lucide icons partout, pas de mix)
- Ombres, borders, radius : uniformes

### 2. Ergonomie
- Actions principales visibles et accessibles (boutons primaires en haut à droite)
- Feedback utilisateur : loading states, empty states, error states
- Formulaires : labels clairs, placeholder utiles, validation inline
- Tables : tri, recherche, pagination, responsive
- Mobile : toutes les pages fonctionnent en responsive

### 3. Accessibilité
- Contraste suffisant (WCAG AA)
- Labels ARIA sur les éléments interactifs
- Navigation clavier (tab order)
- Focus visible sur les éléments focusables

### 4. Performance perçue
- Skeleton loaders pendant le chargement
- Optimistic updates sur les actions fréquentes
- Pas de flash de contenu (layout shift)

## Output

Créer des tâches `todo-front-fix-*` pour chaque problème trouvé (max 8 par cycle).

## Critère de complétion

```
□ Audit de chaque page effectué
□ Liste des problèmes classés par sévérité
□ Tâches créées pour les fixes
□ Renommer en done
```
