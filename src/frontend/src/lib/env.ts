/**
 * Centralised environment variable access for the frontend.
 *
 * NEXT_PUBLIC_API_URL is the base URL of the backend API, visible in the browser.
 * It is intentionally optional: when MSW (Mock Service Worker) is active the
 * variable is left empty and the service worker intercepts all requests before
 * they reach the network.
 *
 * Usage:
 *   import { env } from '@/lib/env'
 *   fetch(`${env.apiUrl}/api/auth/login`)
 */
export const env = {
  /** Base URL of the backend API. Empty string when running in MSW / offline mode. */
  apiUrl: process.env.NEXT_PUBLIC_API_URL ?? '',
} as const
