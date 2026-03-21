# done-front-a11y-001.md — Audit accessibilité (WCAG AA)

**Module** : Frontend
**Dépendances** : aucune
**Priorité** : MOYENNE
**Skills à lire** : `shadcn-nextjs`

---

## Objectif

S'assurer que le produit est accessible (WCAG AA). UAE market = anglais + arabe (RTL). next-intl est déjà configuré.

---

## Résultats de l'audit

### 1. Contraste — FIXED
- `text-gray-400` sur fond blanc atteint ~3.5:1 — inférieur au seuil WCAG AA 4.5:1 pour le texte normal.
  - **Fix** : `SignupForm.tsx` — `text-gray-400` → `text-gray-500` (password strength hints)
  - **Fix** : `PricingSection.tsx` — `text-gray-400` sur la note VAT → `text-gray-500`
- `text-muted-foreground` (CSS var `--muted-foreground`) est `oklch(0.556 …)` = ~4.6:1 sur fond blanc. Conforme WCAG AA. Aucune action requise.

### 2. Navigation clavier — FIXED (partiel)
- Tous les éléments `onClick` utilisent `<button>`, `<Button>`, `<Link>` ou `<a>`. Aucun `div/span` avec `onClick` non interactif trouvé.
- `DrugSelector` : les `li[role="option"]` n'avaient pas de `tabIndex` ni `onKeyDown`.
  - **Fix** : `DrugSelector.tsx` — ajout de `tabIndex={0}` et `onKeyDown` (Enter/Space) sur chaque option.
- `PricingSection` billing toggle buttons : manque `aria-pressed`.
  - **Fix** : `PricingSection.tsx` — ajout de `aria-pressed` sur les deux boutons Monthly/Annual.

### 3. ARIA — FIXED
- **AppointmentsTable** : `SelectTrigger` filtre statut et `<Input type="date">` sans label ni `aria-label`.
  - **Fix** : ajout `aria-label="Filter by status"` et `aria-label="Filter by date"`.
- **InvoiceForm** : inputs de ligne (description, qty, unit price) répétés sans label associé (le `<Label>` n'est rendu que sur `index === 0`). Bouton `×` de suppression sans nom accessible.
  - **Fix** : `aria-label` dynamique sur chaque input de ligne + `aria-label="Remove item N"` sur le bouton `×`.
- **ConversationFilters** : `<Input>` de recherche sans label ni `aria-label`. Icône `Search` sans `aria-hidden`.
  - **Fix** : `aria-label={t('search_placeholder')}` + `aria-hidden="true"` sur l'icône.
- **MessageBubble** : 3 boutons timestamp (toggle relative/full) sans nom accessible.
  - **Fix** : `aria-label` dynamique selon l'état `showFullDate`.
- **DayHoursRow** : `<Input type="time">` (ouverture et fermeture) dans un `<td>` sans label visible.
  - **Fix** : `aria-label` composé de `{dayName} — {col_open/col_close}` en utilisant les clés i18n existantes.
- Éléments déjà conformes : `TemplateFormDialog`, `DispenseToggle`, `ReplyComposer`, `FaqSection`, `DrugSelector` (label row).

### 4. Images et icônes — OK
- `MessageBubble` : `<img>` avec `alt={attachment.fileName}` — conforme.
- `PhotoUpload` : `<img>` avec `alt={photo.file.name}` — conforme.
- Icônes décoratives (ChevronDown, Check, AlertTriangle, Send, StickyNote…) ont déjà `aria-hidden="true"`.
- Icônes fonctionnelles (boutons icon-only) ont déjà `aria-label`.

### 5. RTL (arabe) — OK
- `src/frontend/src/app/[locale]/layout.tsx` ligne 43 : `<html lang={locale} dir={locale === 'ar' ? 'rtl' : 'ltr'}>` — conforme.
- Les composants utilisent `me-` / `ms-` (logical CSS) au lieu de `mr-` / `ml-` dans les contextes RTL sensibles.

### 6. Data-testid — OK
- Déjà couvert par l'audit précédent. Tous les éléments interactifs ont un `data-testid`.

---

## Fichiers modifiés

- `src/frontend/src/components/features/appointments/AppointmentsTable.tsx` — aria-label sur SelectTrigger et date input
- `src/frontend/src/components/features/billing/InvoiceForm.tsx` — aria-label sur line item inputs + remove button
- `src/frontend/src/components/features/messaging/ConversationFilters.tsx` — aria-label + aria-hidden icône search
- `src/frontend/src/components/features/messaging/MessageBubble.tsx` — aria-label sur 3 boutons timestamp
- `src/frontend/src/components/features/messaging/admin/DayHoursRow.tsx` — aria-label sur time inputs
- `src/frontend/src/components/features/patients/DrugSelector.tsx` — tabIndex + onKeyDown sur listbox options
- `src/frontend/src/components/features/auth/SignupForm.tsx` — text-gray-400 → text-gray-500
- `src/frontend/src/components/features/landing/PricingSection.tsx` — text-gray-400 → text-gray-500 + aria-pressed

---

## Critère de complétion

```
[x] Audit contraste effectué
[x] Audit navigation clavier effectué
[x] Audit ARIA effectué
[x] Audit RTL effectué
[x] Audit data-testid effectué
[x] Fixes directs appliqués (< 10 fichiers)
[x] npm run build — 0 erreur TypeScript
[x] Renommer en done
```
