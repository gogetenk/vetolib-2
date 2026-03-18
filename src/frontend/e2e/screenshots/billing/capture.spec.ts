import { test, type Page } from '@playwright/test'
import * as path from 'path'

const SCREENSHOT_DIR = path.resolve(__dirname)

// Reuse the cookie-based auth pattern from the existing billing tests
async function authenticate(page: Page) {
  await page.context().addCookies([
    {
      name: 'access_token',
      value: 'mock-token-for-testing',
      domain: 'localhost',
      path: '/',
      httpOnly: false,
      secure: false,
    },
  ])
  await page.addInitScript(() => {
    localStorage.setItem('access_token', 'mock-token-for-testing')
  })
}

async function waitForMSW(page: Page) {
  await page.waitForSelector('[data-testid="msw-ready"]', {
    state: 'attached',
    timeout: 30000,
  })
}

async function screenshot(page: Page, name: string) {
  await page.screenshot({
    path: path.join(SCREENSHOT_DIR, name),
    fullPage: true,
  })
}

// Run serially to avoid overwhelming the dev server
test.describe.configure({ mode: 'serial' })

test.describe('Billing & Stock — Screenshot Capture', () => {
  // Increase timeout — dev server + MSW init can be slow
  test.setTimeout(60000)

  test.beforeEach(async ({ page }) => {
    await authenticate(page)
  })

  // ── Billing List ──────────────────────────────────────────────────────────

  test('01 — billing list desktop', async ({ page }) => {
    await page.goto('/en/billing')
    await waitForMSW(page)
    await page.waitForSelector('[data-testid="invoice-table"]', { timeout: 15000 })
    await screenshot(page, '01-billing-list.png')
  })

  test('02 — billing list mobile', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 812 })
    await page.goto('/en/billing')
    await waitForMSW(page)
    await page.waitForSelector('[data-testid="invoice-table"]', { timeout: 15000 })
    await screenshot(page, '02-billing-list-mobile.png')
  })

  // ── New Invoice ───────────────────────────────────────────────────────────

  test('03 — new invoice empty', async ({ page }) => {
    await page.goto('/en/billing/new')
    await waitForMSW(page)
    await page.waitForSelector('[data-testid="invoice-form"]', { timeout: 15000 })
    await screenshot(page, '03-invoice-new.png')
  })

  test('04 — new invoice filled', async ({ page }) => {
    await page.goto('/en/billing/new')
    await waitForMSW(page)
    await page.waitForSelector('[data-testid="invoice-form"]', { timeout: 15000 })

    // Select patient
    await page.getByTestId('patient-search-input').fill('Max')
    await page.getByTestId('patient-dropdown').waitFor({ timeout: 5000 })
    await page.getByTestId('patient-option-pat-0000-0000-0000-000000000001').click()

    // Fill first line item
    await page.getByTestId('item-description-0').fill('General Consultation')
    await page.getByTestId('item-quantity-0').fill('1')
    await page.getByTestId('item-unit-price-0').fill('200')

    // Add a second line item
    await page.getByTestId('add-item-btn').click()
    await page.getByTestId('item-description-1').fill('Vaccination — Rabies')
    await page.getByTestId('item-quantity-1').fill('1')
    await page.getByTestId('item-unit-price-1').fill('150')

    // Wait for totals to compute
    await page.waitForTimeout(500)
    await screenshot(page, '04-invoice-new-filled.png')
  })

  // ── Invoice Detail ────────────────────────────────────────────────────────

  test('05 — invoice detail', async ({ page }) => {
    await page.goto('/en/billing/inv-0000-0000-0000-000000000001')
    await waitForMSW(page)
    await page.waitForSelector('[data-testid="invoice-detail"]', { timeout: 15000 })
    await screenshot(page, '05-invoice-detail.png')
  })

  // ── Stock List ────────────────────────────────────────────────────────────

  test('06 — stock list desktop', async ({ page }) => {
    await page.goto('/en/stock')
    await waitForMSW(page)
    await page.waitForSelector('[data-testid="stock-table"]', { timeout: 15000 })
    await screenshot(page, '06-stock-list.png')
  })

  test('07 — stock list mobile', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 812 })
    await page.goto('/en/stock')
    await waitForMSW(page)
    await page.waitForSelector('[data-testid="stock-table"]', { timeout: 15000 })
    await screenshot(page, '07-stock-list-mobile.png')
  })

  // ── Arabic locale ─────────────────────────────────────────────────────────

  test('08 — billing arabic', async ({ page }) => {
    await page.goto('/ar/billing')
    await waitForMSW(page)
    await page.waitForSelector('[data-testid="invoice-table"]', { timeout: 15000 })
    await screenshot(page, '08-billing-ar.png')
  })

  test('09 — stock arabic', async ({ page }) => {
    await page.goto('/ar/stock')
    await waitForMSW(page)
    await page.waitForSelector('[data-testid="stock-table"]', { timeout: 15000 })
    await screenshot(page, '09-stock-ar.png')
  })
})
