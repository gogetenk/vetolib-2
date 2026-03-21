# todo-front-signup-001.md — Page inscription clinique

**Module** : Frontend
**Dépendances** : done-back-signup-selfservice-001
**Priorité** : HAUTE (post-MVP, acquisition)
[MSW: oui]
[Branchement ultérieur: wire-signup]

---

## Objectif

Page d'inscription publique pour créer une clinique et un compte admin.

## Implémentation

1. **Route : /[locale]/signup**
   - Page publique (pas d'auth)
   - Formulaire : nom clinique, email, mot de passe (2x), téléphone
   - Validation inline (email format, password strength, match)
   - CTA : "Create My Clinic"

2. **Après inscription**
   - Auto-login (JWT dans cookie)
   - Redirect vers /[locale]/dashboard
   - Toast "Welcome! Your 14-day free trial has started."

3. **Lien depuis landing page**
   - Les CTA "Start Free Trial" pointent vers /[locale]/signup

4. **i18n EN + AR**

5. **MSW handler**
   - POST /api/v1/clinics/register → 201 avec JWT mock

6. **Playwright E2E**
   - Inscription OK → redirect dashboard
   - Email déjà pris → erreur
   - Validation password

## Critère

```
□ Page /signup avec formulaire
□ MSW handler pour dev
□ Auto-login après inscription
□ CTAs landing page → /signup
□ i18n EN + AR
□ Playwright tests
□ data-testid sur tous les champs
□ Renommer en done
```
