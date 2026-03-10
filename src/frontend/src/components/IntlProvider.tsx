'use client'

import { NextIntlClientProvider } from 'next-intl'
import enMessages from '../../messages/en.json'
import type { ReactNode } from 'react'

/** Provides next-intl context for the legacy (non-locale) dashboard routes. */
export function IntlProvider({ children }: { children: ReactNode }) {
  return (
    <NextIntlClientProvider locale="en" messages={enMessages}>
      {children}
    </NextIntlClientProvider>
  )
}
