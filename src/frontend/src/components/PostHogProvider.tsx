'use client'

import { useEffect, useRef } from 'react'
import { usePathname, useSearchParams } from 'next/navigation'
import { PostHogProvider as PHProvider } from 'posthog-js/react'
import { posthog, initPostHog, isPostHogAvailable } from '@/lib/posthog'

function PostHogPageview() {
  const pathname = usePathname()
  const searchParams = useSearchParams()
  const initialized = useRef(false)

  useEffect(() => {
    if (!isPostHogAvailable()) return

    if (!initialized.current) {
      initPostHog()
      initialized.current = true

      // Restore consent state from localStorage
      const consent = localStorage.getItem('analytics_consent')
      if (consent === 'granted') {
        posthog.opt_in_capturing()
      }
      // 'denied' or null: opt_out is already the default (opt_out_capturing_by_default: true)
    }

    const url =
      pathname +
      (searchParams.toString() ? `?${searchParams.toString()}` : '')
    posthog.capture('$pageview', { $current_url: url })
  }, [pathname, searchParams])

  return null
}

export function PostHogProvider({ children }: { children: React.ReactNode }) {
  if (!isPostHogAvailable()) {
    return <>{children}</>
  }

  return (
    <PHProvider client={posthog}>
      <PostHogPageview />
      {children}
    </PHProvider>
  )
}
