# next-intl — Internationalisation EN/AR avec RTL

## Règles absolues Vetolib

1. **Deux locales uniquement** : `en` (défaut) et `ar`
2. **Aucun texte français** — toute chaîne doit être dans `messages/en.json` et `messages/ar.json`
3. **RTL automatique** — `dir="rtl"` sur `<html>` quand locale = `ar`
4. **Tailwind RTL** — utiliser `ms-*`/`me-*`/`ps-*`/`pe-*`, jamais `ml-`/`mr-`/`pl-`/`pr-`

## Setup

```typescript
// src/i18n/routing.ts
import { defineRouting } from 'next-intl/routing'
export const routing = defineRouting({
  locales: ['en', 'ar'],
  defaultLocale: 'en',
})

// src/i18n/request.ts
import { getRequestConfig } from 'next-intl/server'
import { routing } from './routing'
export default getRequestConfig(async ({ requestLocale }) => {
  const locale = (await requestLocale) ?? routing.defaultLocale
  return {
    locale,
    messages: (await import(`../../messages/${locale}.json`)).default,
  }
})

// middleware.ts
import createMiddleware from 'next-intl/middleware'
import { routing } from './src/i18n/routing'
export default createMiddleware(routing)
export const config = { matcher: ['/((?!api|_next|.*\\..*).*)'] }
```

## Layout avec dir

```typescript
// app/[locale]/layout.tsx
import { NextIntlClientProvider } from 'next-intl'
import { getMessages } from 'next-intl/server'
import { notFound } from 'next/navigation'
import { routing } from '@/i18n/routing'

export default async function LocaleLayout({
  children,
  params: { locale },
}: {
  children: React.ReactNode
  params: { locale: string }
}) {
  if (!routing.locales.includes(locale as any)) notFound()
  const messages = await getMessages()

  return (
    <html lang={locale} dir={locale === 'ar' ? 'rtl' : 'ltr'}>
      <body>
        <NextIntlClientProvider messages={messages}>
          {children}
        </NextIntlClientProvider>
      </body>
    </html>
  )
}
```

## Utilisation dans les composants

```typescript
// Server Component
import { getTranslations } from 'next-intl/server'
export default async function AppointmentsPage() {
  const t = await getTranslations('appointments')
  return <h1>{t('title')}</h1>
}

// Client Component
'use client'
import { useTranslations } from 'next-intl'
export function SubmitButton() {
  const t = useTranslations('common')
  return <button>{t('save')}</button>
}
```

## Sélecteur de langue

```typescript
// components/LanguageSwitcher.tsx
'use client'
import { useRouter, usePathname } from 'next/navigation'
import { useLocale } from 'next-intl'

export function LanguageSwitcher() {
  const locale = useLocale()
  const router = useRouter()
  const pathname = usePathname()

  const switchTo = locale === 'en' ? 'ar' : 'en'
  const label = locale === 'en' ? 'عربي' : 'English'

  const handleSwitch = () => {
    // Remplacer le préfixe de locale dans l'URL courante
    const newPath = pathname.replace(`/${locale}`, `/${switchTo}`)
    router.push(newPath)
  }

  return (
    <button
      data-testid="lang-switcher"
      onClick={handleSwitch}
      className="text-sm font-medium hover:underline"
    >
      {label}
    </button>
  )
}
```

## Règles Tailwind RTL

```
❌  ml-4  mr-2  pl-3  pr-6  left-0  text-left  border-l
✅  ms-4  me-2  ps-3  pe-6  start-0  text-start  border-s

Pourquoi : avec dir="rtl", ms-4 devient automatiquement margin-right.
Avec ml-4, la marge reste à gauche même en arabe (layout cassé).
```

## Formatage des dates et montants (UAE)

```typescript
// Dates en arabe : utiliser le calendrier grégorien (pas hijri pour le MVP)
import { useFormatter, useLocale } from 'next-intl'

export function FormattedDate({ date }: { date: Date }) {
  const format = useFormatter()
  const locale = useLocale()

  return (
    <span>
      {format.dateTime(date, {
        year: 'numeric', month: 'long', day: 'numeric',
        // Timezone UAE
        timeZone: 'Asia/Dubai',
      })}
    </span>
  )
}

// Montants en AED
export function FormattedAmount({ amount }: { amount: number }) {
  const format = useFormatter()
  return (
    <span>
      {format.number(amount, { style: 'currency', currency: 'AED' })}
    </span>
  )
}
```

## Structure messages/

```
messages/
├── en.json   ← source of truth (écrire EN en premier)
└── ar.json   ← traduction complète (même structure que en.json)
```

Règle : chaque clé dans `en.json` doit exister dans `ar.json`.
Un lint check peut être ajouté pour vérifier la complétude.

## Anti-patterns

```
❌ Texte hardcodé dans un composant : <span>Save</span>
✅ Toujours via t() : <span>{t('common.save')}</span>

❌ Détecter la langue côté client avec navigator.language
✅ Laisser next-intl gérer via le middleware et les cookies

❌ Deux layouts différents pour EN et AR
✅ Un seul layout, dir="rtl" géré par l'attribut HTML

❌ Images ou icônes non-symétrisées en RTL
✅ Vérifier que les chevrons/flèches s'inversent (CSS transform en RTL)
```
