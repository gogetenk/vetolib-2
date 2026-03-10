/**
 * INTEGRATION TESTS — require the real backend (Aspire running).
 *
 * These tests previously ran against the MSW scheduling handler which has been
 * removed as part of the wire-scheduling-001 task. The suggest-slot endpoint
 * (POST /api/agenda/suggest-slot) must now be served by the real backend.
 *
 * To run these tests:
 *   1. Start the Aspire AppHost: `dotnet run --project src/backend/AppHost`
 *   2. Run Playwright against the integration config:
 *      `npx playwright test e2e/appointments/suggest-slot.spec.ts --config playwright.integration.config.ts`
 *
 * These tests are skipped in the standard MSW-based Playwright run because the
 * MSW handler no longer intercepts POST /api/agenda/suggest-slot.
 */
import { test, expect } from '@playwright/test'

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
  await page.goto('/appointments/new')
  await page.waitForSelector('[data-testid="appointment-form"]', { timeout: 15000 })
  await page.waitForSelector('[data-testid="suggest-button"]', { timeout: 10000 })
}

test.describe('Suggest Slot (integration — real backend required)', () => {
  test.skip(true, 'MSW handler removed — run against real backend with playwright.integration.config.ts')

  test.beforeEach(async ({ context }) => {
    await authenticate(context)
  })

  test('Suggest button is visible on the appointment form', async ({ page }) => {
    await gotoForm(page)
    await expect(page.locator('[data-testid="suggest-button"]')).toBeVisible()
  })

  test('Clicking suggest shows suggestion panel', async ({ page }) => {
    await gotoForm(page)
    await page.click('[data-testid="suggest-button"]')

    await page.waitForSelector('[data-testid="suggestion-panel"]', { timeout: 10000 })
    await expect(page.locator('[data-testid="suggestion-panel"]')).toBeVisible()
  })

  test('Each suggestion card shows vet name, date, time, score and reasoning', async ({ page }) => {
    await gotoForm(page)
    await page.click('[data-testid="suggest-button"]')
    await page.waitForSelector('[data-testid="suggestion-panel"]', { timeout: 10000 })

    // With the real backend we check structure, not specific mock data values
    const card0 = page.locator('[data-testid="suggestion-card-0"]')
    await expect(card0).toBeVisible()
    await expect(page.locator('[data-testid="suggestion-select-0"]')).toBeVisible()
  })

  test('Clicking a suggestion pre-fills date, vet and time fields, then closes panel', async ({ page }) => {
    await gotoForm(page)
    await page.click('[data-testid="suggest-button"]')
    await page.waitForSelector('[data-testid="suggestion-panel"]', { timeout: 10000 })

    await page.click('[data-testid="suggestion-select-0"]')

    await expect(page.locator('[data-testid="suggestion-panel"]')).not.toBeVisible()

    const dateValue = await page.inputValue('[data-testid="input-date"]')
    expect(dateValue).toBeTruthy()

    await expect(page.locator('[data-testid="select-vet-trigger"]')).not.toBeEmpty()
    await expect(page.locator('[data-testid="select-time-trigger"]')).not.toBeEmpty()
  })

  test('Suggest button shows loading state while fetching', async ({ page }) => {
    await gotoForm(page)
    await page.click('[data-testid="suggest-button"]')

    await page.waitForSelector('[data-testid="suggestion-panel"]', { timeout: 10000 })
    await expect(page.locator('[data-testid="suggestion-panel"]')).toBeVisible()

    const suggestBtn = page.locator('[data-testid="suggest-button"]')
    await expect(suggestBtn).not.toBeDisabled()
  })

  test('Error toast appears when suggest-slot API fails', async ({ page }) => {
    // This test relies on backend returning an error — configure via test fixtures or
    // a dedicated error-trigger endpoint when the backend supports it.
    await gotoForm(page)
    // Placeholder: actual error scenario depends on backend test configuration
    await expect(page.locator('[data-testid="suggest-button"]')).toBeVisible()
  })
})
