'use client'

import { useEffect } from 'react'

/**
 * Registers the Vetara PWA service worker.
 * Only registers in production (or when explicitly enabled) to avoid
 * conflicting with MSW's mockServiceWorker.js during development.
 */
export function useServiceWorker() {
  useEffect(() => {
    if (
      typeof window === 'undefined' ||
      !('serviceWorker' in navigator) ||
      process.env.NODE_ENV === 'development'
    ) {
      return
    }

    navigator.serviceWorker
      .register('/sw.js')
      .catch((err) => {
        console.error('[Vetara SW] registration failed:', err)
      })
  }, [])
}
