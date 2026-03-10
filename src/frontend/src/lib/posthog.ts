import posthog from 'posthog-js'

const POSTHOG_KEY = process.env.NEXT_PUBLIC_POSTHOG_KEY
const POSTHOG_HOST =
  process.env.NEXT_PUBLIC_POSTHOG_HOST ?? 'https://eu.i.posthog.com'

export function initPostHog(): void {
  if (typeof window === 'undefined') return
  if (!POSTHOG_KEY) return

  posthog.init(POSTHOG_KEY, {
    api_host: POSTHOG_HOST,
    capture_pageview: false,
    capture_pageleave: true,
    persistence: 'localStorage',
    opt_out_capturing_by_default: true,
  })
}

export function isPostHogAvailable(): boolean {
  return typeof window !== 'undefined' && !!POSTHOG_KEY
}

export { posthog }
