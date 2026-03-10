import { test, expect, type Page } from '@playwright/test'

// These tests require the real backend (wire task done-wire-stock-001).
// Run against a live Aspire environment: NEXT_PUBLIC_API_URL must be set.
test.skip(true, 'Integration tests — require real backend (Aspire). Run with playwright.integration.config.ts.')

// Minimal JWT token for testing
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

const VET_TOKEN = makeToken('VET', 'Dr. Sarah Johnson', 'dr.sarah@desertpaws.ae')
const ADMIN_TOKEN = makeToken('ADMIN', 'Ahmed Al-Rashid', 'admin@desertpaws.ae')

async function waitForMSW(page: Page): Promise<void> {
  await page.waitForSelector('[data-testid="msw-ready"]', { state: 'attached', timeout: 15000 })
}

async function loginAs(page: Page, token: string, targetPath = '/en/stock'): Promise<void> {
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

// ────────────────────────────────────────────────────────────────────────────
// Stock List
// ────────────────────────────────────────────────────────────────────────────
test.describe('Stock page — VET', () => {
  test.beforeEach(async ({ page }) => {
    await loginAs(page, VET_TOKEN, '/en/stock')
    await page.waitForSelector('[data-testid="stock-page"]', { timeout: 15000 })
    await page.waitForSelector('[data-testid="stock-table"]', { timeout: 10000 })
  })

  test('displays stock page title', async ({ page }) => {
    await expect(page.getByTestId('stock-title')).toBeVisible()
  })

  test('shows Add Item button', async ({ page }) => {
    await expect(page.getByTestId('btn-add-stock-item')).toBeVisible()
  })

  test('shows stock table with rows', async ({ page }) => {
    const rows = page.locator('[data-testid^="stock-row-"]')
    await expect(rows).toHaveCount(8)
  })

  test('shows low-stock badge for Amoxicillin (qty 5 < threshold 20)', async ({ page }) => {
    const amoxicillinRow = page.locator('[data-testid="stock-row-stock-0000-0000-0000-000000000001"]')
    await expect(amoxicillinRow).toBeVisible()
    await expect(page.getByTestId('badge-low-stock-stock-0000-0000-0000-000000000001')).toBeVisible()
  })

  test('shows expiring badge for Rabies Vaccine (expiry < 30 days from 2026-03-09)', async ({ page }) => {
    await expect(page.getByTestId('badge-expiring-stock-0000-0000-0000-000000000002')).toBeVisible()
  })

  test('shows alerts banner with low-stock items', async ({ page }) => {
    await expect(page.getByTestId('alert-low-stock')).toBeVisible()
  })

  test('shows alerts banner with expiring items', async ({ page }) => {
    await expect(page.getByTestId('alert-expiring')).toBeVisible()
  })
})

// ────────────────────────────────────────────────────────────────────────────
// Filters
// ────────────────────────────────────────────────────────────────────────────
test.describe('Stock filters', () => {
  test.beforeEach(async ({ page }) => {
    await loginAs(page, VET_TOKEN, '/en/stock')
    await page.waitForSelector('[data-testid="stock-table"]', { timeout: 10000 })
  })

  test('filter by category Medication shows only medication rows', async ({ page }) => {
    await page.getByTestId('filter-category-trigger').click()
    await page.getByTestId('filter-category-medication').click()
    const rows = page.locator('[data-testid^="stock-row-"]')
    // Medications: Amoxicillin, Meloxicam, Diphenhydramine = 3
    await expect(rows).toHaveCount(3)
  })

  test('filter by status low-stock shows only low-stock rows', async ({ page }) => {
    await page.getByTestId('filter-status-trigger').click()
    await page.getByTestId('filter-status-low-stock').click()
    const rows = page.locator('[data-testid^="stock-row-"]')
    // Low stock: Amoxicillin, FVRCP Vaccine, Diphenhydramine = 3
    await expect(rows).toHaveCount(3)
  })
})

// ────────────────────────────────────────────────────────────────────────────
// Add stock item
// ────────────────────────────────────────────────────────────────────────────
test.describe('Add stock item', () => {
  test.beforeEach(async ({ page }) => {
    await loginAs(page, ADMIN_TOKEN, '/en/stock')
    await page.waitForSelector('[data-testid="stock-table"]', { timeout: 10000 })
  })

  test('opens add dialog and creates a new item', async ({ page }) => {
    await page.getByTestId('btn-add-stock-item').click()
    await expect(page.getByTestId('stock-item-dialog')).toBeVisible()

    await page.getByTestId('input-stock-name').fill('Ivermectin 1%')
    await page.getByTestId('select-stock-category-trigger').click()
    await page.getByTestId('category-option-medication').click()
    await page.getByTestId('input-stock-quantity').fill('50')
    await page.getByTestId('input-stock-unit').fill('vials')
    await page.getByTestId('input-stock-threshold').fill('10')

    await page.getByTestId('btn-save-stock-item').click()

    // Dialog should close and new item appear
    await expect(page.getByTestId('stock-item-dialog')).not.toBeVisible()
    const rows = page.locator('[data-testid^="stock-row-"]')
    await expect(rows).toHaveCount(9)
  })

  test('shows validation error when name is empty', async ({ page }) => {
    await page.getByTestId('btn-add-stock-item').click()
    await page.getByTestId('btn-save-stock-item').click()
    await expect(page.getByTestId('error-stock-name')).toBeVisible()
  })

  test('cancel button closes dialog without adding', async ({ page }) => {
    await page.getByTestId('btn-add-stock-item').click()
    await page.getByTestId('btn-cancel-stock-item').click()
    await expect(page.getByTestId('stock-item-dialog')).not.toBeVisible()
    const rows = page.locator('[data-testid^="stock-row-"]')
    await expect(rows).toHaveCount(8)
  })
})

// ────────────────────────────────────────────────────────────────────────────
// Edit stock item
// ────────────────────────────────────────────────────────────────────────────
test.describe('Edit stock item', () => {
  test.beforeEach(async ({ page }) => {
    await loginAs(page, VET_TOKEN, '/en/stock')
    await page.waitForSelector('[data-testid="stock-table"]', { timeout: 10000 })
  })

  test('opens edit dialog with prefilled values', async ({ page }) => {
    await page.getByTestId('btn-edit-stock-0000-0000-0000-000000000001').click()
    await expect(page.getByTestId('stock-item-dialog')).toBeVisible()
    // Amoxicillin name should be prefilled
    await expect(page.getByTestId('input-stock-name')).toHaveValue('Amoxicillin 250mg')
  })

  test('updates item name and saves', async ({ page }) => {
    await page.getByTestId('btn-edit-stock-0000-0000-0000-000000000001').click()
    await page.getByTestId('input-stock-name').fill('Amoxicillin 500mg')
    await page.getByTestId('btn-save-stock-item').click()
    await expect(page.getByTestId('stock-item-dialog')).not.toBeVisible()
    await expect(page.getByTestId('stock-name-stock-0000-0000-0000-000000000001')).toContainText('Amoxicillin 500mg')
  })
})

// ────────────────────────────────────────────────────────────────────────────
// Stock movement
// ────────────────────────────────────────────────────────────────────────────
test.describe('Stock movement', () => {
  test.beforeEach(async ({ page }) => {
    await loginAs(page, VET_TOKEN, '/en/stock')
    await page.waitForSelector('[data-testid="stock-table"]', { timeout: 10000 })
  })

  test('opens movement dialog for Meloxicam', async ({ page }) => {
    await page.getByTestId('btn-movement-stock-0000-0000-0000-000000000003').click()
    await expect(page.getByTestId('stock-movement-dialog')).toBeVisible()
    await expect(page.getByTestId('movement-current-qty')).toBeVisible()
  })

  test('records IN movement and updates quantity', async ({ page }) => {
    await page.getByTestId('btn-movement-stock-0000-0000-0000-000000000003').click()
    // Type is IN by default
    await page.getByTestId('input-movement-quantity').fill('20')
    await page.getByTestId('input-movement-reason').fill('Monthly restock')
    await page.getByTestId('btn-save-movement').click()
    await expect(page.getByTestId('stock-movement-dialog')).not.toBeVisible()
    // Meloxicam quantity was 80, +20 = 100
    await expect(page.getByTestId('stock-quantity-stock-0000-0000-0000-000000000003')).toContainText('100')
  })

  test('records OUT movement', async ({ page }) => {
    await page.getByTestId('btn-movement-stock-0000-0000-0000-000000000003').click()
    await page.getByTestId('select-movement-type-trigger').click()
    await page.getByTestId('movement-type-out').click()
    await page.getByTestId('input-movement-quantity').fill('10')
    await page.getByTestId('btn-save-movement').click()
    await expect(page.getByTestId('stock-movement-dialog')).not.toBeVisible()
    // 80 - 10 = 70
    await expect(page.getByTestId('stock-quantity-stock-0000-0000-0000-000000000003')).toContainText('70')
  })

  test('shows validation error when OUT exceeds current stock', async ({ page }) => {
    // Amoxicillin has only 5 units
    await page.getByTestId('btn-movement-stock-0000-0000-0000-000000000001').click()
    await page.getByTestId('select-movement-type-trigger').click()
    await page.getByTestId('movement-type-out').click()
    await page.getByTestId('input-movement-quantity').fill('100')
    await page.getByTestId('btn-save-movement').click()
    await expect(page.getByTestId('error-movement-quantity')).toBeVisible()
  })

  test('cancel movement dialog closes without change', async ({ page }) => {
    await page.getByTestId('btn-movement-stock-0000-0000-0000-000000000003').click()
    await page.getByTestId('btn-cancel-movement').click()
    await expect(page.getByTestId('stock-movement-dialog')).not.toBeVisible()
  })
})
