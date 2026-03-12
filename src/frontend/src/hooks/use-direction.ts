'use client'

import { useLocale } from 'next-intl'

/**
 * Returns 'rtl' or 'ltr' based on current locale.
 * Use to conditionally flip arrow icons, layout, etc.
 */
export function useDirection(): 'rtl' | 'ltr' {
  const locale = useLocale()
  return locale === 'ar' ? 'rtl' : 'ltr'
}
