# todo-front-i18n-001.md — Frontend : Internationalisation AR + EN

**Dépendances** : done-front-scaffold-000
**Skills** : `shadcn-nextjs`
[MSW: non — pas d'impact API]

---

## Objectif

Internationaliser le frontend Vetolib en **anglais (EN)** et **arabe (AR)**.
Supprimer tout texte français hardcodé.
L'arabe est RTL — le layout doit s'inverser automatiquement.

---

## Stack i18n

```bash
npm install next-intl
```

`next-intl` est la solution standard pour Next.js App Router avec support RTL natif.

---

## Structure

```
vetolib-frontend/
├── messages/
│   ├── en.json      ← toutes les chaînes EN
│   └── ar.json      ← toutes les chaînes AR
├── src/
│   ├── i18n/
│   │   ├── routing.ts       ← config locales + defaultLocale
│   │   └── request.ts       ← getRequestConfig
│   └── middleware.ts        ← ajouter la détection de locale
└── next.config.ts           ← plugin next-intl
```

## Configuration

```typescript
// src/i18n/routing.ts
import { defineRouting } from 'next-intl/routing'

export const routing = defineRouting({
  locales: ['en', 'ar'],
  defaultLocale: 'en',
})
```

```typescript
// middleware.ts — fusionner avec la protection auth existante
import createMiddleware from 'next-intl/middleware'
import { routing } from './i18n/routing'

const intlMiddleware = createMiddleware(routing)

export default function middleware(request: NextRequest) {
  // 1. i18n routing
  // 2. auth protection (existant)
}
```

## URLs avec locale

```
/en/login         ← anglais
/ar/login         ← arabe (RTL)
/en/appointments
/ar/appointments
```

## Support RTL

Next-intl gère la locale, mais le RTL nécessite :

```typescript
// app/[locale]/layout.tsx
export default async function LocaleLayout({ children, params: { locale } }) {
  return (
    <html lang={locale} dir={locale === 'ar' ? 'rtl' : 'ltr'}>
      <body>{children}</body>
    </html>
  )
}
```

Tailwind RTL — ajouter dans `tailwind.config.ts` :
```typescript
// Les classes Tailwind s'inversent automatiquement avec dir="rtl"
// ms-* (margin-start) au lieu de ml-* (margin-left)
// ps-* (padding-start) au lieu de pl-*
// Vérifier tous les composants et remplacer l-*/r-* par s-*/e-*
```

## Fichiers de traduction (contenu minimum)

```json
// messages/en.json
{
  "auth": {
    "login": {
      "title": "Sign In",
      "email": "Email",
      "password": "Password",
      "submit": "Sign In",
      "errors": {
        "invalid_credentials": "Invalid email or password",
        "account_locked": "Account locked. Try again in 15 minutes."
      }
    }
  },
  "nav": {
    "appointments": "Appointments",
    "patients": "Patients",
    "medical_records": "Medical Records",
    "billing": "Billing",
    "team": "Team",
    "sign_out": "Sign Out"
  },
  "appointments": {
    "title": "Appointments",
    "new": "New Appointment",
    "status": {
      "SCHEDULED": "Scheduled",
      "CHECKED_IN": "Checked In",
      "IN_PROGRESS": "In Progress",
      "COMPLETED": "Completed",
      "CANCELLED": "Cancelled"
    }
  },
  "patients": {
    "title": "Patients",
    "species": {
      "Dog": "Dog", "Cat": "Cat", "Bird": "Bird",
      "Rabbit": "Rabbit", "Horse": "Horse",
      "Camel": "Camel", "Exotic": "Exotic"
    }
  },
  "billing": {
    "title": "Billing",
    "subtotal": "Subtotal",
    "vat": "VAT (5%)",
    "total": "Total"
  },
  "common": {
    "save": "Save",
    "cancel": "Cancel",
    "confirm": "Confirm",
    "loading": "Loading...",
    "error": "Something went wrong"
  }
}
```

```json
// messages/ar.json
{
  "auth": {
    "login": {
      "title": "تسجيل الدخول",
      "email": "البريد الإلكتروني",
      "password": "كلمة المرور",
      "submit": "تسجيل الدخول",
      "errors": {
        "invalid_credentials": "البريد الإلكتروني أو كلمة المرور غير صحيحة",
        "account_locked": "تم تجميد الحساب. حاول مرة أخرى بعد 15 دقيقة."
      }
    }
  },
  "nav": {
    "appointments": "المواعيد",
    "patients": "الحيوانات",
    "medical_records": "السجلات الطبية",
    "billing": "الفواتير",
    "team": "الفريق",
    "sign_out": "تسجيل الخروج"
  },
  "appointments": {
    "title": "المواعيد",
    "new": "موعد جديد",
    "status": {
      "SCHEDULED": "مجدول",
      "CHECKED_IN": "تم الوصول",
      "IN_PROGRESS": "قيد الفحص",
      "COMPLETED": "مكتمل",
      "CANCELLED": "ملغى"
    }
  },
  "patients": {
    "title": "الحيوانات",
    "species": {
      "Dog": "كلب", "Cat": "قطة", "Bird": "طائر",
      "Rabbit": "أرنب", "Horse": "حصان",
      "Camel": "جمل", "Exotic": "حيوان نادر"
    }
  },
  "billing": {
    "title": "الفواتير",
    "subtotal": "المجموع الجزئي",
    "vat": "ضريبة القيمة المضافة (5%)",
    "total": "الإجمالي"
  },
  "common": {
    "save": "حفظ",
    "cancel": "إلغاء",
    "confirm": "تأكيد",
    "loading": "جارٍ التحميل...",
    "error": "حدث خطأ ما"
  }
}
```

## Sélecteur de langue

Dans le `Header.tsx` (composant shell existant) :
```typescript
// Bouton simple EN | عربي dans le header, à côté du UserMenu
// Clic → navigate vers la même page avec l'autre locale
// Stocker la préférence dans un cookie (next-intl le gère nativement)
```

## Migration des composants existants

Remplacer tous les textes hardcodés par `useTranslations()` :
```typescript
// Avant
<h1>Appointments</h1>

// Après
const t = useTranslations('appointments')
<h1>{t('title')}</h1>
```

Remplacer toutes les classes `ml-*`/`mr-*`/`pl-*`/`pr-*` par `ms-*`/`me-*`/`ps-*`/`pe-*` pour le RTL.

## Tests Playwright

```typescript
// e2e/i18n/rtl.spec.ts
test('Arabic layout is RTL', async ({ page }) => {
  await page.goto('/ar/login')
  const html = page.locator('html')
  await expect(html).toHaveAttribute('dir', 'rtl')
  await expect(html).toHaveAttribute('lang', 'ar')
})

test('Language switcher changes locale', async ({ page }) => {
  await page.goto('/en/appointments')
  await page.getByTestId('lang-switcher-ar').click()
  await expect(page).toHaveURL('/ar/appointments')
  await expect(page.getByTestId('page-title')).toContainText('المواعيد')
})
```

## Critère de complétion

```
□ next-intl configuré avec locales ['en', 'ar']
□ messages/en.json et messages/ar.json couvrent tous les écrans
□ /ar/* → dir="rtl", /en/* → dir="ltr"
□ Sélecteur de langue fonctionnel dans le header
□ Aucun texte français hardcodé dans les composants
□ Tailwind RTL : ms-*/me-*/ps-*/pe-* à la place de l-*/r-*
□ Tests Playwright RTL verts
□ npm run build → 0 erreur
□ Renommer en done-front-i18n-001.md
```
