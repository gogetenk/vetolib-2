# todo-ux-critical-017.md -- Fix CRITICAL UX issues

**Module** : Frontend
**Priority** : Critique
**Dependencies** : aucune

## Scope

### U-01 CRITICAL: Fix brand name "Veto" → "Vetara" in Header.tsx
- File: src/frontend/src/components/features/shell/Header.tsx line 52

### U-02 CRITICAL: Internationalize Help Center
- File: src/frontend/src/app/[locale]/help/page.tsx
- Extract all 890 lines of English text to next-intl messages

### U-03 CRITICAL: Internationalize Profile page
- File: src/frontend/src/app/[locale]/(dashboard)/profile/ProfilePageClient.tsx

### U-04 HIGH: Add missing French i18n keys (27 keys)
- Update src/frontend/messages/fr.json

## Completion criteria
- [ ] Brand name fixed
- [ ] Help Center uses next-intl
- [ ] Profile page uses next-intl
- [ ] French translations complete
- [ ] `npm run lint` + `npm run build` GREEN
