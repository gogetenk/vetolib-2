# todo-front-qa-fix-001 -- Fix QA bloquants frontend

**Module** : Frontend
**Priority** : Critique
**Dependencies** : none

## Problemes a corriger

### 1. AppointmentDetailSheet.tsx -- 12+ chaines en francais (CRITIQUE)
Fichier : `src/frontend/src/components/features/calendar/AppointmentDetailSheet.tsx`
Toutes les chaines francaises doivent passer par next-intl (useTranslations).

### 2. Redirect sans locale (CRITIQUE)
- `src/frontend/src/lib/api/client.ts` lignes 84, 148, 187 : `window.location.href = "/login"` -> `/en/login` ou mieux, utiliser le locale courant
- `src/frontend/src/lib/auth.ts` ligne 39 : idem

### 3. 6 select natifs a remplacer par shadcn Select
- ConversationActions.tsx (205)
- MessageClassification.tsx (185, 201)
- CategorySelector.tsx (30)
- PetSelector.tsx (20)
- StepConsultationType.tsx (167)

### 4. data-testid manquants sur CalendarHeader
Boutons Today, Prev, Next, New Appointment et vets sans data-testid.

### 5. AppointmentDetailSheet data-testid
Instrumenter les champs principaux.

### 6. SettingsPageClient bg-[#303ef5]
Remplacer par bg-primary (si pas deja fait par PR #152).

## Verification
```bash
cd src/frontend && npm run lint && npm run build
```
