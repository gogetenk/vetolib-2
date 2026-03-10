/**
 * Messaging fixtures for Playwright tests.
 *
 * Provides helpers to:
 *  - Login as different roles (receptionist, vet, admin, assistant)
 *  - Navigate to the inbox
 *  - Open the owner portal with a magic link token
 *
 * All tests run against MSW — no real backend required.
 */
import type { Page } from '@playwright/test'

// ─── Minimal JWT token factory ──────────────────────────────────────────────
// Role encoded in payload — MSW handlers use the role from the token.

function makeToken(role: string, name: string, email: string): string {
  const payload = {
    sub: email,
    name,
    role,
    clinicId: 'clinic-001',
    clinicName: 'Desert Paws Clinic',
    exp: Math.floor(Date.now() / 1000) + 3600,
  }
  const header = btoa(JSON.stringify({ alg: 'HS256', typ: 'JWT' })).replace(/=/g, '')
  const body = btoa(JSON.stringify(payload)).replace(/=/g, '')
  return `${header}.${body}.fake-signature`
}

export const TOKENS = {
  receptionist: makeToken('RECEPTIONIST', 'Khalid Al-Nuaimi', 'reception@desertpaws.ae'),
  vet: makeToken('VET', 'Dr. Sarah Johnson', 'dr.sarah@desertpaws.ae'),
  admin: makeToken('ADMIN', 'Admin User', 'admin@desertpaws.ae'),
  assistant: makeToken('ASSISTANT', 'Mariam Al-Zaabi', 'assistant@desertpaws.ae'),
}

// ─── Mock conversation IDs (from MOCK_CONVERSATIONS in data/messaging.ts) ────

export const CONV_IDS = {
  medicalUrgency: 'conv-0000-0000-0000-000000000001',     // MedicalUrgency, Critical, Open
  postOp: 'conv-0000-0000-0000-000000000002',             // PostOperativeFollowUp, High, InProgress, 6 messages (has summary)
  appointmentRequest: 'conv-0000-0000-0000-000000000003', // AppointmentRequest, Normal, Open
  administrative: 'conv-0000-0000-0000-000000000004',     // Administrative, Low, Resolved
  adminEnquiry: 'conv-0000-0000-0000-000000000005',       // Administrative, Low, Open, triageUncertain
  medicalQuestion: 'conv-0000-0000-0000-000000000006',    // MedicalQuestion, Normal, InProgress, 4 messages (Arabic)
  feedback: 'conv-0000-0000-0000-000000000007',           // Feedback, Low, Resolved
  spam: 'conv-0000-0000-0000-000000000008',               // spam=true, Closed
}

// ─── Wait for MSW service worker ─────────────────────────────────────────────

async function waitForMSW(page: Page): Promise<void> {
  await page.waitForSelector('[data-testid="msw-ready"]', {
    state: 'attached',
    timeout: 15000,
  })
}

// ─── Auth helpers ─────────────────────────────────────────────────────────────

async function loginWithToken(page: Page, token: string, targetPath: string): Promise<void> {
  await page.context().addCookies([
    {
      name: 'access_token',
      value: token,
      domain: 'localhost',
      path: '/',
      httpOnly: false,
      secure: false,
    },
  ])
  await page.addInitScript((t) => {
    localStorage.setItem('access_token', t)
  }, token)
  await page.goto(targetPath)
  await waitForMSW(page)
}

export async function loginAsReceptionist(page: Page, targetPath = '/en/messages'): Promise<void> {
  await loginWithToken(page, TOKENS.receptionist, targetPath)
}

export async function loginAsVet(page: Page, targetPath = '/en/messages'): Promise<void> {
  await loginWithToken(page, TOKENS.vet, targetPath)
}

export async function loginAsAdmin(page: Page, targetPath = '/en/messages'): Promise<void> {
  await loginWithToken(page, TOKENS.admin, targetPath)
}

export async function loginAsAssistant(page: Page, targetPath = '/en/messages'): Promise<void> {
  await loginWithToken(page, TOKENS.assistant, targetPath)
}

// ─── Navigation helpers ───────────────────────────────────────────────────────

export async function navigateToInbox(page: Page): Promise<void> {
  await page.goto('/en/messages')
  await waitForMSW(page)
  await page.waitForSelector('[data-testid="messages-page"]', { timeout: 10000 })
}

// ─── Portal helpers ───────────────────────────────────────────────────────────

/**
 * Navigate to the owner portal with the given magic link token.
 * The token is set via the ?token= query string, which the portal layout
 * stores in sessionStorage before the component mounts.
 */
export async function openPortalWithMagicLink(page: Page, token: string): Promise<void> {
  await page.goto(`/en/portal/desert-paws?token=${token}`)
  await waitForMSW(page)
}
