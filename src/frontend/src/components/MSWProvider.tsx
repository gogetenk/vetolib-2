'use client'
import { useEffect, useState } from 'react'

/**
 * Initialises the MSW browser service worker in development.
 *
 * In development (NODE_ENV=development), children are NOT rendered until the
 * MSW service worker is active. This ensures all API calls made from child
 * useEffect hooks are intercepted by MSW.
 *
 * A hidden [data-testid="msw-ready"] sentinel is rendered once MSW is active.
 * Playwright tests should wait for this sentinel before asserting data-driven UI:
 *   await page.waitForSelector('[data-testid="msw-ready"]', { state: 'attached', timeout: 15000 })
 *
 * In production (NODE_ENV=production), MSW is never started and children render
 * immediately (mswReady defaults to true).
 *
 * Wire tasks (Playwright tests without MSW): set window.__DISABLE_MSW__ = true via
 * page.addInitScript() to skip MSW initialization and render children immediately.
 */
export function MSWProvider({ children }: { children: React.ReactNode }) {
  // Always start false so server and client render the same initial HTML.
  const [mswReady, setMswReady] = useState(false)

  useEffect(() => {
    // In production or when explicitly disabled (wire Playwright tests), skip MSW.
    if (
      process.env.NODE_ENV !== 'development' ||
      (window as unknown as Record<string, unknown>).__DISABLE_MSW__ === true
    ) {
      setMswReady(true)
      return
    }

    import('@/mocks/browser').then(({ worker }) =>
      worker.start({ onUnhandledRequest: 'bypass' })
    ).then(() => setMswReady(true))
  }, [])

  // In development, don't render children until MSW is active so that
  // useEffect API calls are guaranteed to be intercepted by MSW handlers.
  const shouldRenderChildren =
    mswReady || process.env.NODE_ENV !== 'development'

  return (
    <>
      {mswReady && (
        <span
          data-testid="msw-ready"
          style={{ display: 'none' }}
          aria-hidden="true"
        />
      )}
      {shouldRenderChildren ? children : null}
    </>
  )
}
