# QA Report — Auth & Landing

## Test Results
- Passed: ~37 (11 landing + 10 signup + 7 login + 9 recette auth)
- Skipped: 0
- Failed: 0

## Coverage Analysis

### Pages covered
- `/` (landing) — hero, social proof, features, pricing toggle, FAQ accordion, testimonials, footer, JSON-LD, AR/RTL
- `/login` — successful login, wrong password, unknown email, account lockout, empty form validation, already-authenticated redirect, sign out
- `/signup` — form rendering, successful registration, email taken, password strength, password mismatch, empty form, AR/RTL, CTA links

### Pages/features NOT covered
- Landing page mobile hamburger menu (if any)
- Login "forgot password" flow (not implemented)

## Bugs Found

### [BUG-001] LoginForm has ZERO i18n — all strings hardcoded in English
- **Severity**: High
- **File**: `src/components/features/auth/LoginForm.tsx`
- **Description**: LoginForm does not call `useTranslations()` at all. Every string is hardcoded: "Email", "Password", "Sign In", "Signing in...", "Veterinary Management", error messages. Meanwhile `SignupForm` properly uses `useTranslations("auth.signup")`.
- **Expected**: LoginForm should use `useTranslations("auth.login")` with keys in en.json/ar.json

### [BUG-002] LoginForm redirects without locale prefix
- **Severity**: High
- **File**: `src/components/features/auth/LoginForm.tsx:44`
- **Description**: `router.push("/appointments")` has no locale prefix. Should be `/${locale}/appointments`. Causes double redirect via middleware.
- **Expected**: Import `useLocale` and use locale-prefixed route

### [BUG-003] Landing page hardcoded scroll hint
- **Severity**: Low
- **File**: `src/app/[locale]/page.tsx:534`
- **Description**: "Scroll to see more" is hardcoded English, not using i18n. Only visible on mobile.

### [BUG-004] LoginForm Zod validation messages hardcoded
- **Severity**: Medium
- **File**: `src/components/features/auth/LoginForm.tsx:22-24`
- **Description**: Zod schema messages "Please enter a valid email address" and "Password is required" are hardcoded. Compare with SignupForm which builds its schema with t().

## Missing data-testid
- No gaps found — all interactive elements have data-testid

## i18n Issues
- LoginForm: complete i18n gap (BUG-001, BUG-004)
- Landing scroll hint: 1 hardcoded string (BUG-003)
- SignupForm: properly internationalized
- Landing page: properly internationalized

## a11y Issues
- No significant gaps found. Error messages have role="alert", fields have aria-invalid.

## Recommendations
1. Fix BUG-001 + BUG-004 (High): Add useTranslations to LoginForm
2. Fix BUG-002 (High): Add useLocale and locale-prefixed route
3. Fix BUG-003 (Low): Add scroll hint to i18n
