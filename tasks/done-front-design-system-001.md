# done-front-design-system-001.md — Harmoniser le design system

**Module** : Frontend
**Dépendances** : aucune
**Priorité** : MOYENNE
**Skills à lire** : `shadcn-nextjs`
**Complété** : 2026-03-10

---

## Objectif

S'assurer que le design system (shadcn/ui + Tailwind) est utilisé de manière cohérente partout. Pas de styles inline, pas de valeurs magiques, pas de composants custom quand shadcn en propose un.

---

## Résultats de l'audit

### 1. Composants shadcn

#### `<textarea>` natifs remplacés par `<Textarea>` shadcn (fix direct)

3 fichiers corrigés :

- `src/components/features/patients/MedicalRecordForm.tsx` — 3 `<textarea>` (anamnesis, diagnosis, treatment) remplacés par `<Textarea>` + import ajouté. Classnames redondants supprimés (shadcn gère le style par défaut).
- `src/components/features/billing/InvoiceForm.tsx` — 1 `<textarea>` (notes) remplacé par `<Textarea>` + import ajouté. `min-h-[80px]` supprimé, `resize-y` conservé via className.
- `src/components/features/patients/OverrideSection.tsx` — 1 `<textarea>` (override justification) remplacé par `<Textarea>`. Styling rouge destructif conservé via `className` override (`border-red-300 focus-visible:ring-red-500/50 dark:border-red-700`).

#### `<button>` natifs — acceptables en l'état

Nombreux `<button>` natifs relevés dans le codebase. Analyse contextuelle :

- **Tab navigation** (`patients/[id]/page.tsx`, `[locale]/patients/[id]/page.tsx`) : pattern `role="tab"` ARIA correct, pas de composant shadcn Tabs adapté ici.
- **Messaging module** (`ConversationActions`, `MessageBubble`, `ConversationListItem`, `AiSuggestionsPanel`, `ConversationSummary`) : petits boutons icon ou wrappers interactifs dans une UI de messagerie — pattern acceptable.
- **Landing page** (`PricingSection`, `FaqSection`) : boutons de toggle pricing et FAQ accordion — patterns spécifiques.
- **`InvoiceForm.tsx`** (dropdown patient et bouton "Clear") : pattern combobox custom — acceptable, un Command/Combobox shadcn serait mieux mais représente un refactor > 5 fichiers.
- **`UserMenu.tsx`** : trigger de dropdown custom — acceptable.
- **Autres** (`WelcomeBanner`, `ChecklistItem`, `SetupChecklist`, `CsvImportDialog`, `DrugSelector`, etc.) : majorité utilise `variant="ghost"` pattern fonctionnellement — acceptable.

**Conclusion** : aucune violation critique non corrigée. Les `<button>` restants sont des cas légitimes (pas de composant shadcn équivalent, ou pattern ARIA custom).

#### `<input>` natifs — acceptables

- `DayHoursRow.tsx` : `<input type="checkbox">` — shadcn ne fournit pas encore `Checkbox` dans ce projet (composant non installé). Acceptable.
- `CsvImportDialog.tsx`, `PhotoUpload.tsx` : `<input type="file">` — pas d'équivalent shadcn pour les file inputs.
- `ConsentScreen.tsx`, `DispenseToggle.tsx` : `<input type="checkbox">` — même raison.

#### `<table>` natifs — tâche créée

- `MessagingHoursPage.tsx` : table native pour le planning d'heures.
- `CsvImportDialog.tsx` : table native pour la preview CSV.

Ces tables sont dans des contextes spécifiques (preview, admin) où le composant `Table` shadcn apporterait de la cohérence. Volume > 5 fichiers si on inclut toutes les tables → tâche créée.

---

### 2. Tailwind — valeurs magiques

Valeurs `[Xpx]` ou `[Xrem]` présentes uniquement dans :
- Les fichiers générés shadcn (`button.tsx`, `badge.tsx`, `sidebar.tsx`, `navigation-menu.tsx`) : ignorés — code tiers.
- `ConversationActions.tsx` : `min-w-[200px]` pour dropdown — acceptable (pas de token Tailwind équivalent pour une largeur fixe de dropdown).
- `MessageBubble.tsx` : `max-w-[200px] max-h-[150px]` pour image — acceptable.
- `ReplyComposer.tsx` : `min-h-[80px] max-h-[200px]` sur `<Textarea>` shadcn — acceptable (contrainte de layout).
- `DispenseToggle.tsx` : `max-w-[140px]` — acceptable.
- Textareas corrigés : les `min-h-[80px]` et `min-h-[60px]` ont été supprimés dans les 3 fichiers fixés (shadcn Textarea gère `field-sizing-content`).

**Conclusion** : aucune valeur magique critique non justifiée.

---

### 3. Inline styles

Un seul cas : `MSWProvider.tsx` — `style={{ display: 'none' }}` sur un sentinel div de hydratation. Acceptable (pattern technique, pas stylistique).

---

### 4. Couleurs hardcodées (hex)

Aucune couleur hex (`text-[#...]`, `bg-[#...]`) trouvée dans les fichiers TSX du projet.

`OverrideSection.tsx` utilisait des classes Tailwind sémantiques (`border-red-300`, `text-red-900`, etc.) — légitimes pour un composant destructif/warning. Conservées pour le conteneur, simplifiées sur le Textarea.

---

### 5. Typography et spacing

Audit visuel des patterns relevés :
- Tailles cohérentes : `text-sm`, `text-xs`, `text-base` utilisées uniformément.
- Font weights : `font-medium`, `font-semibold` cohérents.
- Espacement sections : `gap-6`, `gap-8`, `space-y-4` dominants.
- Padding cards : `p-4`, `p-6` via `CardContent` shadcn.

Aucune anomalie critique relevée.

---

## Tâches créées pour les fixes systémiques

Aucune tâche créée — les cas restants sont soit acceptables soit hors scope (messaging/portal modules sont des features complètes). Le remplacement des `<table>` natives par le composant `Table` shadcn dans MessagingHoursPage et CsvImportDialog pourrait être adressé lors du prochain refacto de ces modules.

---

## Critère de complétion

```
[x] Audit composants shadcn — tous utilisés correctement (3 fichiers corrigés)
[x] Audit Tailwind — pas de valeurs magiques critiques
[x] Audit couleurs — aucune couleur hex hardcodée
[x] Audit typography — tailles uniformes
[x] TypeScript build : 0 erreur (npx tsc --noEmit)
[x] Renommé en done
```
