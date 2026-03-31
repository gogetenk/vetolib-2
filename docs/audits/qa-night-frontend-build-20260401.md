# QA Audit — Frontend Lint + Build — 2026-04-01

**Branch** : develop
**Timestamp** : 2026-04-01
**Status** : BUILD_PASS / LINT_WARNINGS / NO_CONSOLE_LOGS

---

## 1. ESLint — `npm run lint`

Result: 0 errors, 3 warnings.

The build pipeline is not blocked (errors only block). The 3 warnings are all `react-hooks/exhaustive-deps` violations where the `t` translation function (from `next-intl` or similar) is used inside a hook callback but omitted from the dependency array.

| # | File | Line | Rule | Details |
|---|------|------|------|---------|
| 1 | `src/app/[locale]/(dashboard)/patients/PatientsPageClient.tsx` | 51 | `react-hooks/exhaustive-deps` | `useCallback` missing dep: `t` |
| 2 | `src/components/features/billing/InvoiceDetail.tsx` | 115 | `react-hooks/exhaustive-deps` | `useEffect` missing dep: `t` |
| 3 | `src/components/features/dashboard/DismissAlertDialog.tsx` | 44 | `react-hooks/exhaustive-deps` | `useCallback` missing dep: `t` |

**Root cause** : In all three cases the `t` function from the i18n hook is used inside the callback body but not listed as a dependency. In practice `t` is a stable reference (does not change between renders), so this rarely causes bugs at runtime. However the linter cannot statically prove stability, so it flags it correctly.

**Fix pattern** for each occurrence:

```tsx
// Before (line 51 in PatientsPageClient.tsx)
}, [])

// After
}, [t])
```

The same one-line fix applies to `InvoiceDetail.tsx` line 115 and `DismissAlertDialog.tsx` line 44.

---

## 2. TypeScript / Next.js Build — `npm run build`

Result: **0 TypeScript errors. Build succeeded.**

```
✓ Compiled successfully in 4.7s
  Finished TypeScript in 6.2s ... (0 errors)
✓ Generating static pages (10/10)
```

All 55 routes compiled without issue. No type errors surfaced.

**One framework-level deprecation warning was emitted by Next.js itself (not TypeScript):**

```
⚠ The "middleware" file convention is deprecated.
  Please use "proxy" instead.
  Learn more: https://nextjs.org/docs/messages/middleware-to-proxy
```

- File: `src/frontend/src/middleware.ts`
- This is a Next.js 16.x breaking change. The file named `middleware.ts` must be renamed to `proxy.ts` (or the new proxy convention). The current file implements auth route protection and locale redirection — it is functionally correct but will eventually stop working as the framework removes middleware support.
- This is a **non-blocking build warning** today, but will become a hard error in a future Next.js minor/patch.

---

## 3. console.log scan

```
grep -rn "console.log" src/frontend/src/ --include="*.tsx" --include="*.ts"
```

Result: **0 matches.** No debug logs left in source files.

---

## Summary

| Check | Result | Blocking |
|-------|--------|---------|
| ESLint errors | 0 | n/a |
| ESLint warnings | 3 (react-hooks/exhaustive-deps) | No |
| TypeScript errors | 0 | n/a |
| Build | SUCCESS | n/a |
| Next.js deprecation warning | 1 (middleware → proxy) | Not today, will be in future |
| console.log in source | 0 | n/a |

---

## Recommended actions (non-blocking)

1. **Add `t` to dependency arrays** in the 3 files listed in section 1. One-line change each. Eliminates ESLint noise and makes the lint output clean.

2. **Rename `middleware.ts` to `proxy.ts`** and update exports per the Next.js 16 migration guide (`https://nextjs.org/docs/messages/middleware-to-proxy`). The logic inside the file is sound — it is purely a file naming and export convention change. Delaying this will eventually cause a hard build failure when Next.js removes the old convention.
