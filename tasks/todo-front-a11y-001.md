# todo-front-a11y-001.md — Audit accessibilité (WCAG AA)

**Module** : Frontend
**Dépendances** : aucune
**Priorité** : MOYENNE
**Skills à lire** : `shadcn-nextjs`

---

## Objectif

S'assurer que le produit est accessible (WCAG AA). UAE market = anglais + arabe (RTL). next-intl est déjà configuré.

## Checks

### 1. Contraste
- Texte sur fond : ratio minimum 4.5:1 (normal) / 3:1 (large)
- Boutons : texte lisible sur fond coloré
- Placeholder text : assez contrasté pour être lu

### 2. Navigation clavier
- Tab order logique sur toutes les pages
- Focus visible (outline) sur tous les éléments interactifs
- Escape ferme les dialogs/dropdowns
- Enter soumet les formulaires

### 3. ARIA
- Labels sur les inputs (`aria-label` ou `<label>` associé)
- Rôles sur les éléments custom (`role="button"`, `role="dialog"`)
- `aria-expanded` sur les accordions/dropdowns
- `aria-live` pour les notifications/toasts

### 4. Images et icônes
- Alt text sur toutes les images
- Icônes décoratives : `aria-hidden="true"`
- Icônes fonctionnelles : `aria-label` descriptif

### 5. RTL (arabe)
- Vérifier que le layout se retourne correctement en arabe
- Icônes directionnelles (flèches) se retournent
- Texte aligné à droite en arabe

### 6. Data-testid
- Vérifier que TOUS les éléments interactifs ont un `data-testid` (obligatoire CLAUDE.md)

## Critère de complétion

```
□ Audit contraste effectué
□ Audit navigation clavier effectué
□ Audit ARIA effectué
□ Audit RTL effectué
□ Audit data-testid effectué
□ Tâches créées pour les fixes
□ Renommer en done
```
