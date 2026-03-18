# todo-front-auth-redesign-001.md — Redesign Auth zone (landing + login + signup)

**Dépendances** : aucune
**Skills** : shadcn-nextjs
**[MSW: oui]**

## Objectif
The Auth zone scored 2.7/10 — the lowest of all zones. The "landing page" is just the login form, the "signup page" is identical to login, and there are no validation states or password strength UI visible. This task redesigns the entire auth experience.

## What to build

### 1. Real landing page (`/[locale]/page.tsx`)
- Hero section with value proposition: "Veterinary clinic management for the UAE"
- Feature highlights (3-4 cards): Appointments, Patient Records, Billing, Messaging
- Social proof / testimonials section
- CTA buttons: "Get Started" → signup, "Sign In" → login
- Top navigation bar: Logo, Language Switcher (EN/AR), Login, Sign Up
- Footer: Privacy Policy, Terms, Contact

### 2. Improved login page (`/[locale]/login/page.tsx`)
- Keep the card-centered layout but add:
  - Logo (not just text "Vetolib")
  - "Forgot password?" link below password field
  - "Don't have an account? Sign up" link below button
  - Password visibility toggle (eye icon)
  - Error messages above the button (not below)
  - Field-level validation with red borders on error
  - `role="alert"` on error messages

### 3. Ensure signup form is properly differentiated
- The SignupForm component exists with proper fields (clinicName, email, phone, password, confirmPassword)
- Verify the route renders it correctly (not the login form)
- Ensure password strength indicator is visible
- Ensure validation errors display inline

### 4. Language switcher component
- EN/AR toggle in the top-right (or top-left in RTL)
- Visible on all auth pages
- Persists locale choice

## Important
- Landing page and signup MUST be accessible without authentication (fix middleware if needed)
- All text must use i18n keys (useTranslations)
- AR translations included for all new strings

## Critère de complétion
- [ ] Landing page has hero, features, CTAs, nav, footer
- [ ] Login page has forgot password, signup link, password toggle, proper errors
- [ ] Signup page renders correctly with all fields and validation
- [ ] Language switcher works on auth pages
- [ ] All pages accessible without authentication
- [ ] AR translations present for all new strings
- [ ] `npm run build` passes
