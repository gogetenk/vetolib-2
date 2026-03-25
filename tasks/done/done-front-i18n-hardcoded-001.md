# todo-front-i18n-hardcoded-001 -- Fix remaining hardcoded English strings

**Module** : Frontend
**Priority** : Haute
**Dependencies** : none

## Context

Several components still have hardcoded English strings instead of using next-intl translations.
UAE market requires EN+AR support.

## Scope

Search for hardcoded English strings in:
- `src/frontend/src/components/`
- `src/frontend/src/app/`

Focus on user-visible text (button labels, headings, error messages, placeholders).
Do NOT touch code comments or console.log messages.

## Rules

- Use existing next-intl setup: `useTranslations('namespace')`
- Add translation keys to existing message files in `src/frontend/messages/`
- Keep all `data-testid` attributes
- `npm run lint` + `npm run build` GREEN

## Completion criteria

- [ ] No hardcoded English strings in user-visible UI text
- [ ] Translation keys added to EN message file
- [ ] `npm run lint` GREEN
- [ ] `npm run build` GREEN
