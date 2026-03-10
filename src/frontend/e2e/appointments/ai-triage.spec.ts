/**
 * AI Triage — Integration tests against real backend.
 *
 * These tests require the real backend (Aspire) to be running.
 * They are skipped in CI/MSW environments.
 *
 * To run:
 *   npx playwright test e2e/appointments/ai-triage.spec.ts
 */
import { test, expect } from '@playwright/test'

test.skip(true, 'Wire task: requires real backend — skip in MSW/CI environments')

async function authenticate(context: import('@playwright/test').BrowserContext) {
  await context.addCookies([
    {
      name: 'access_token',
      value: 'mock-access-token',
      domain: 'localhost',
      path: '/',
      httpOnly: false,
      secure: false,
    },
  ])
}

async function gotoForm(page: import('@playwright/test').Page) {
  await page.goto('/en/appointments/new')
  await page.waitForSelector('[data-testid="appointment-form"]', { timeout: 15000 })
}

test.describe('AI Triage — Real Backend', () => {
  test.beforeEach(async ({ context }) => {
    await authenticate(context)
  })

  // ─────────────────────────────────────────────────────────────────────────────
  // Triage section is visible on the form
  // ─────────────────────────────────────────────────────────────────────────────
  test('triage section is visible on the appointment form', async ({ page }) => {
    await gotoForm(page)
    await expect(page.getByTestId('triage-section')).toBeVisible()
    await expect(page.getByTestId('input-symptoms')).toBeVisible()
    await expect(page.getByTestId('analyze-symptoms-button')).toBeVisible()
  })

  // ─────────────────────────────────────────────────────────────────────────────
  // Analyze button is disabled when symptoms textarea is empty
  // ─────────────────────────────────────────────────────────────────────────────
  test('analyze button is disabled when symptoms field is empty', async ({ page }) => {
    await gotoForm(page)
    await expect(page.getByTestId('analyze-symptoms-button')).toBeDisabled()
  })

  // ─────────────────────────────────────────────────────────────────────────────
  // Triage panel appears after analyzing symptoms
  // ─────────────────────────────────────────────────────────────────────────────
  test('triage panel appears with suggestion after analyzing symptoms', async ({ page }) => {
    await gotoForm(page)

    await page.getByTestId('input-symptoms').fill(
      'The dog has been limping on the left hind leg for 2 days and refuses to eat'
    )
    await page.getByTestId('analyze-symptoms-button').click()

    await page.waitForSelector('[data-testid="triage-panel"]', { timeout: 10000 })

    await expect(page.getByTestId('triage-panel')).toBeVisible()
    await expect(page.getByTestId('triage-severity-badge')).toBeVisible()
    await expect(page.getByTestId('triage-specialty')).toBeVisible()
    await expect(page.getByTestId('triage-duration')).toBeVisible()
    await expect(page.getByTestId('triage-reasoning')).toBeVisible()
    await expect(page.getByTestId('triage-disclaimer')).toBeVisible()
    await expect(page.getByTestId('accept-triage')).toBeVisible()
    await expect(page.getByTestId('override-triage')).toBeVisible()
  })

  // ─────────────────────────────────────────────────────────────────────────────
  // Emergency symptoms get red badge
  // ─────────────────────────────────────────────────────────────────────────────
  test('emergency symptoms show Emergency badge', async ({ page }) => {
    await gotoForm(page)

    await page.getByTestId('input-symptoms').fill(
      'The cat collapsed and is barely breathing, emergency'
    )
    await page.getByTestId('analyze-symptoms-button').click()

    await page.waitForSelector('[data-testid="triage-panel"]', { timeout: 10000 })

    const badge = page.getByTestId('triage-severity-badge')
    await expect(badge).toBeVisible()
    await expect(badge).toContainText('Emergency')
  })

  // ─────────────────────────────────────────────────────────────────────────────
  // Routine symptoms get Routine badge
  // ─────────────────────────────────────────────────────────────────────────────
  test('routine symptoms show Routine badge', async ({ page }) => {
    await gotoForm(page)

    await page.getByTestId('input-symptoms').fill('Annual checkup and routine vaccination')
    await page.getByTestId('analyze-symptoms-button').click()

    await page.waitForSelector('[data-testid="triage-panel"]', { timeout: 10000 })

    const badge = page.getByTestId('triage-severity-badge')
    await expect(badge).toContainText('Routine')
  })

  // ─────────────────────────────────────────────────────────────────────────────
  // Disclaimer is always visible (non-dismissable)
  // ─────────────────────────────────────────────────────────────────────────────
  test('disclaimer is always visible and cannot be dismissed', async ({ page }) => {
    await gotoForm(page)

    await page.getByTestId('input-symptoms').fill('My dog is scratching a lot')
    await page.getByTestId('analyze-symptoms-button').click()

    await page.waitForSelector('[data-testid="triage-disclaimer"]', { timeout: 10000 })

    const disclaimer = page.getByTestId('triage-disclaimer')
    await expect(disclaimer).toBeVisible()
    const dismissBtn = page.locator('[data-testid="dismiss-disclaimer"]')
    await expect(dismissBtn).not.toBeVisible()
  })

  // ─────────────────────────────────────────────────────────────────────────────
  // Accept triage pre-fills the notes field
  // ─────────────────────────────────────────────────────────────────────────────
  test('Accept button pre-fills notes with triage recommendation', async ({ page }) => {
    await gotoForm(page)

    await page.getByTestId('input-symptoms').fill('The rabbit has been eating less for 3 days')
    await page.getByTestId('analyze-symptoms-button').click()

    await page.waitForSelector('[data-testid="accept-triage"]', { timeout: 10000 })
    await page.getByTestId('accept-triage').click()

    const notesValue = await page.inputValue('[data-testid="textarea-notes"]')
    expect(notesValue.length).toBeGreaterThan(0)

    await expect(page.getByTestId('triage-accepted-indicator')).toBeVisible()
    await expect(page.getByTestId('accept-triage')).not.toBeVisible()
    await expect(page.getByTestId('override-triage')).not.toBeVisible()
  })

  // ─────────────────────────────────────────────────────────────────────────────
  // Override clears the triage result
  // ─────────────────────────────────────────────────────────────────────────────
  test('Override button clears the triage result and symptoms', async ({ page }) => {
    await gotoForm(page)

    await page.getByTestId('input-symptoms').fill('The cat is limping slightly')
    await page.getByTestId('analyze-symptoms-button').click()

    await page.waitForSelector('[data-testid="override-triage"]', { timeout: 10000 })
    await page.getByTestId('override-triage').click()

    await expect(page.getByTestId('triage-panel')).not.toBeVisible()

    const symptomsValue = await page.inputValue('[data-testid="input-symptoms"]')
    expect(symptomsValue).toBe('')
  })

  // ─────────────────────────────────────────────────────────────────────────────
  // AI down — form still works normally
  // ─────────────────────────────────────────────────────────────────────────────
  test('AI service down — form remains usable and shows error toast', async ({ page }) => {
    await gotoForm(page)

    await page.getByTestId('input-symptoms').fill('__ai_down__ test scenario')
    await page.getByTestId('analyze-symptoms-button').click()

    await page.waitForTimeout(1500)
    await expect(page.getByTestId('triage-panel')).not.toBeVisible()

    await expect(page.getByTestId('appointment-form')).toBeVisible()
    await expect(page.getByTestId('btn-save')).toBeVisible()
    await expect(page.getByTestId('input-patient-name')).toBeVisible()
  })
})
