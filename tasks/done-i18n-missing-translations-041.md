# Task: Add missing FR and AR translations

**Module:** Frontend
**Priority:** HIGH (legal-critical for UAE market)
**Source:** docs/audits/qa-night-i18n-20260401.md

## Problem
- fr.json missing 76 keys vs en.json (including entire Terms + Privacy sections)
- ar.json missing 74 keys vs en.json (same gaps)
- Terms of Service and Privacy Policy are LEGAL REQUIREMENTS — must be translated

## Fix
1. Read src/frontend/messages/en.json for the complete key set
2. Read src/frontend/messages/fr.json and add all missing keys with proper French translations
3. Read src/frontend/messages/ar.json and add all missing keys with proper Arabic translations
4. Focus on: auth.signup (5 keys), terms.* (34 keys), privacy.* (36 keys), landing.testimonials (6 keys)
5. Translations must be professional quality — these are legal/marketing texts

## Definition of Done
- [ ] `npm run build` passes (no missing key warnings)
- [ ] fr.json key count matches en.json
- [ ] ar.json key count matches en.json
- [ ] Terms and Privacy fully translated in FR and AR
