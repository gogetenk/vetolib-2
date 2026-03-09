import { test, expect } from '@playwright/test'

// Helper to set auth cookie and mock token so middleware lets us through
async function authenticate(page: import('@playwright/test').Page) {
  // Set the access_token cookie that the middleware checks
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
  // Also set localStorage token (used by the API client)
  await page.addInitScript(() => {
    localStorage.setItem('access_token', 'mock-token-for-testing')
  })
}

// Wait for MSW service worker to be ready before asserting data-driven UI
async function waitForMSW(page: import('@playwright/test').Page) {
  // msw-ready has display:none so we check for DOM attachment, not visibility
  await page.waitForSelector('[data-testid="msw-ready"]', { state: 'attached', timeout: 15000 })
}

test.describe('Billing — Invoice List', () => {
  test.beforeEach(async ({ page }) => {
    await authenticate(page)
    await page.goto('/billing')
    await waitForMSW(page)
    await page.waitForSelector('[data-testid="invoice-table"]', { timeout: 15000 })
  })

  test('displays the billing page with invoice table', async ({ page }) => {
    await expect(page.getByTestId('billing-page')).toBeVisible()
    await expect(page.getByTestId('billing-title')).toHaveText('Billing')
    await expect(page.getByTestId('invoice-table')).toBeVisible()
  })

  test('shows mock invoices with correct data', async ({ page }) => {
    await expect(page.getByTestId('invoice-table')).toContainText('INV-2026-001')
    await expect(page.getByTestId('invoice-table')).toContainText('Max')
    await expect(page.getByTestId('invoice-table')).toContainText('AED')
  })

  test('shows grand total at bottom', async ({ page }) => {
    await expect(page.getByTestId('invoice-grand-total')).toBeVisible()
    await expect(page.getByTestId('invoice-grand-total')).toContainText('AED')
  })

  test('has new invoice button', async ({ page }) => {
    await expect(page.getByTestId('new-invoice-btn')).toBeVisible()
  })

  test('filters invoices by status DRAFT', async ({ page }) => {
    await page.getByTestId('status-filter').selectOption('DRAFT')
    // Wait for re-render
    await page.waitForTimeout(800)
    await expect(page.getByTestId('invoice-table')).toContainText('INV-2026-001')
    // PAID invoice should not appear after filtering
    await expect(page.getByTestId('invoice-table')).not.toContainText('INV-2026-003')
  })

  test('filters invoices by status PAID', async ({ page }) => {
    await page.getByTestId('status-filter').selectOption('PAID')
    await page.waitForTimeout(800)
    await expect(page.getByTestId('invoice-table')).toContainText('INV-2026-003')
    await expect(page.getByTestId('invoice-table')).toContainText('Rocky')
  })

  test('displays correct AED amounts with VAT 5%', async ({ page }) => {
    // First invoice: subtotal 200, VAT 10, total 210
    await expect(page.getByTestId('invoice-table')).toContainText('AED 210.00')
  })
})

test.describe('Billing — Create Invoice', () => {
  test.beforeEach(async ({ page }) => {
    await authenticate(page)
    await page.goto('/billing/new')
    await waitForMSW(page)
    await page.waitForSelector('[data-testid="invoice-form"]', { timeout: 15000 })
  })

  test('displays new invoice page', async ({ page }) => {
    await expect(page.getByTestId('new-invoice-page')).toBeVisible()
    await expect(page.getByTestId('new-invoice-title')).toHaveText('New Invoice')
    await expect(page.getByTestId('invoice-form')).toBeVisible()
  })

  test('shows patient search input', async ({ page }) => {
    await expect(page.getByTestId('patient-search-input')).toBeVisible()
  })

  test('can search and select a patient', async ({ page }) => {
    await page.getByTestId('patient-search-input').fill('Max')
    await expect(page.getByTestId('patient-dropdown')).toBeVisible()
    await page.getByTestId('patient-option-pat-0000-0000-0000-000000000001').click()
    await expect(page.getByTestId('selected-patient')).toBeVisible()
    await expect(page.getByTestId('selected-patient')).toContainText('Max')
    await expect(page.getByTestId('selected-patient')).toContainText('Ahmed Al-Rashid')
  })

  test('has add item button and shows default line item', async ({ page }) => {
    await expect(page.getByTestId('add-item-btn')).toBeVisible()
    await expect(page.getByTestId('line-item-0')).toBeVisible()
    await expect(page.getByTestId('item-description-0')).toBeVisible()
    await expect(page.getByTestId('item-quantity-0')).toBeVisible()
    await expect(page.getByTestId('item-unit-price-0')).toBeVisible()
  })

  test('calculates totals with VAT 5% automatically', async ({ page }) => {
    await page.getByTestId('item-description-0').fill('Consultation')
    await page.getByTestId('item-quantity-0').fill('1')
    await page.getByTestId('item-unit-price-0').fill('200')

    // Subtotal: 200, VAT: 10, Total: 210
    await expect(page.getByTestId('form-subtotal')).toContainText('AED 200.00')
    await expect(page.getByTestId('form-vat')).toContainText('AED 10.00')
    await expect(page.getByTestId('form-total')).toContainText('AED 210.00')
  })

  test('can add multiple items', async ({ page }) => {
    await page.getByTestId('add-item-btn').click()
    await expect(page.getByTestId('line-item-1')).toBeVisible()
    await expect(page.getByTestId('item-description-1')).toBeVisible()
  })

  test('creates invoice and redirects to detail page', async ({ page }) => {
    // Select patient
    await page.getByTestId('patient-search-input').fill('Max')
    await page.getByTestId('patient-dropdown').waitFor()
    await page.getByTestId('patient-option-pat-0000-0000-0000-000000000001').click()

    // Fill item
    await page.getByTestId('item-description-0').fill('Consultation')
    await page.getByTestId('item-quantity-0').fill('1')
    await page.getByTestId('item-unit-price-0').fill('200')

    // Submit
    await page.getByTestId('submit-invoice-btn').click()

    // Should redirect to detail page
    await page.waitForURL(/\/billing\/.+/, { timeout: 10000 })
    await expect(page.getByTestId('invoice-detail-page')).toBeVisible()
  })
})

test.describe('Billing — Invoice Detail & Status Transitions', () => {
  test.beforeEach(async ({ page }) => {
    await authenticate(page)
  })

  test('displays DRAFT invoice with Send/Edit/Delete actions', async ({ page }) => {
    await page.goto('/billing/inv-0000-0000-0000-000000000001')
    await waitForMSW(page)
    await page.waitForSelector('[data-testid="invoice-detail"]', { timeout: 15000 })

    await expect(page.getByTestId('invoice-detail-number')).toContainText('INV-2026-001')
    await expect(page.getByTestId('invoice-detail-status')).toHaveText('Draft')

    await expect(page.getByTestId('send-invoice-btn')).toBeVisible()
    await expect(page.getByTestId('edit-invoice-btn')).toBeVisible()
    await expect(page.getByTestId('delete-invoice-btn')).toBeVisible()

    await expect(page.getByTestId('mark-paid-btn')).not.toBeVisible()
    await expect(page.getByTestId('download-pdf-btn')).not.toBeVisible()
  })

  test('displays SENT invoice with Mark as Paid/Cancel actions', async ({ page }) => {
    await page.goto('/billing/inv-0000-0000-0000-000000000002')
    await waitForMSW(page)
    await page.waitForSelector('[data-testid="invoice-detail"]', { timeout: 15000 })

    await expect(page.getByTestId('invoice-detail-status')).toHaveText('Sent')
    await expect(page.getByTestId('mark-paid-btn')).toBeVisible()
    await expect(page.getByTestId('cancel-invoice-btn')).toBeVisible()

    await expect(page.getByTestId('send-invoice-btn')).not.toBeVisible()
  })

  test('displays PAID invoice with Download PDF action', async ({ page }) => {
    await page.goto('/billing/inv-0000-0000-0000-000000000003')
    await waitForMSW(page)
    await page.waitForSelector('[data-testid="invoice-detail"]', { timeout: 15000 })

    await expect(page.getByTestId('invoice-detail-status')).toHaveText('Paid')
    await expect(page.getByTestId('download-pdf-btn')).toBeVisible()
    await expect(page.getByTestId('invoice-paid-date')).toBeVisible()

    await expect(page.getByTestId('send-invoice-btn')).not.toBeVisible()
    await expect(page.getByTestId('mark-paid-btn')).not.toBeVisible()
  })

  test('transitions DRAFT → SENT when clicking Send', async ({ page }) => {
    await page.goto('/billing/inv-0000-0000-0000-000000000001')
    await waitForMSW(page)
    await page.waitForSelector('[data-testid="send-invoice-btn"]', { timeout: 15000 })

    await page.getByTestId('send-invoice-btn').click()

    await expect(page.getByTestId('invoice-detail-status')).toHaveText('Sent', { timeout: 5000 })
    await expect(page.getByTestId('mark-paid-btn')).toBeVisible()
    await expect(page.getByTestId('cancel-invoice-btn')).toBeVisible()
  })

  test('displays correct AED amounts and VAT on detail page', async ({ page }) => {
    await page.goto('/billing/inv-0000-0000-0000-000000000001')
    await waitForMSW(page)
    await page.waitForSelector('[data-testid="detail-totals"]', { timeout: 15000 })

    await expect(page.getByTestId('detail-subtotal')).toContainText('AED 200.00')
    await expect(page.getByTestId('detail-vat')).toContainText('AED 10.00')
    await expect(page.getByTestId('detail-total')).toContainText('AED 210.00')
  })

  test('displays client info on detail page', async ({ page }) => {
    await page.goto('/billing/inv-0000-0000-0000-000000000001')
    await waitForMSW(page)
    await page.waitForSelector('[data-testid="invoice-detail"]', { timeout: 15000 })

    await expect(page.getByTestId('invoice-owner-name')).toHaveText('Ahmed Al-Rashid')
    await expect(page.getByTestId('invoice-owner-phone')).toHaveText('+971 50 123 4567')
    await expect(page.getByTestId('invoice-patient-name')).toHaveText('Max')
  })

  test('transitions SENT → PAID when clicking Mark as Paid', async ({ page }) => {
    await page.goto('/billing/inv-0000-0000-0000-000000000002')
    await waitForMSW(page)
    await page.waitForSelector('[data-testid="mark-paid-btn"]', { timeout: 15000 })

    await page.getByTestId('mark-paid-btn').click()

    await expect(page.getByTestId('invoice-detail-status')).toHaveText('Paid', { timeout: 5000 })
    await expect(page.getByTestId('invoice-paid-date')).toBeVisible()
    await expect(page.getByTestId('download-pdf-btn')).toBeVisible()
  })
})

test.describe('Billing — Navigation', () => {
  test.beforeEach(async ({ page }) => {
    await authenticate(page)
  })

  test('can navigate from list to detail via View button', async ({ page }) => {
    await page.goto('/billing')
    await waitForMSW(page)
    await page.waitForSelector('[data-testid="invoice-table"]', { timeout: 15000 })

    await page.getByTestId('view-invoice-inv-0000-0000-0000-000000000001').click()
    await page.waitForURL(/\/billing\/inv-0000-0000-0000-000000000001/, { timeout: 5000 })
    await expect(page.getByTestId('invoice-detail-page')).toBeVisible()
  })

  test('can navigate from list to new invoice via button', async ({ page }) => {
    await page.goto('/billing')
    await waitForMSW(page)
    await page.waitForSelector('[data-testid="new-invoice-btn"]', { timeout: 15000 })

    await page.getByTestId('new-invoice-btn').click()
    await page.waitForURL(/\/billing\/new/, { timeout: 5000 })
    await expect(page.getByTestId('new-invoice-page')).toBeVisible()
  })

  test('can go back to billing from detail page', async ({ page }) => {
    await page.goto('/billing/inv-0000-0000-0000-000000000001')
    await waitForMSW(page)
    await page.waitForSelector('[data-testid="back-to-billing-btn"]', { timeout: 15000 })

    await page.getByTestId('back-to-billing-btn').click()
    await page.waitForURL(/\/billing$/, { timeout: 5000 })
    await expect(page.getByTestId('billing-page')).toBeVisible()
  })
})
