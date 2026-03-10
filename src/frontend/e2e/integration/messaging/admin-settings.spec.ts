/**
 * Admin Settings — Messaging — Playwright tests (MSW mode)
 *
 * 3 scenarios:
 *  1. Template CRUD (create, edit, delete)
 *  2. Messaging hours configuration
 *  3. Stats dashboard displays metrics
 */
import { test, expect } from '@playwright/test'
import { loginAsAdmin } from '../../fixtures/messaging'

// Known template ID from MOCK_TEMPLATES
const TEMPLATE_ID_1 = 'tmpl-0000-0000-0000-000000000001'

test.describe('Admin Settings — Templates CRUD', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsAdmin(page, '/en/settings/messaging/templates')
    await page.waitForSelector('[data-testid="templates-page"]', { timeout: 15000 })
  })

  test('Template CRUD: create, edit, delete', async ({ page }) => {
    await expect(page.getByTestId('templates-page')).toBeVisible()
    await expect(page.getByTestId('templates-table')).toBeVisible()

    // ── CREATE ─────────────────────────────────────────────────────────
    await page.getByTestId('add-template-btn').click()

    // Template form dialog should appear
    await expect(page.getByTestId('template-form-dialog')).toBeVisible({ timeout: 5000 })

    // Fill in the form
    await page.getByTestId('template-name-input').fill('Test Template')
    await page.getByTestId('template-content-en-input').fill(
      'Thank you for contacting us. We will respond shortly.'
    )
    await page.getByTestId('template-content-ar-input').fill(
      'شكراً لتواصلك معنا. سنرد عليك قريباً.'
    )

    // Save the template
    await page.getByTestId('template-form-save-btn').click()

    // Dialog should close and new template should appear in table
    await expect(page.getByTestId('template-form-dialog')).not.toBeVisible({ timeout: 5000 })

    // The new template should be in the table
    const rows = page.locator('[data-testid^="template-row-"]')
    const initialCount = await rows.count()
    expect(initialCount).toBeGreaterThan(0)

    // ── EDIT ──────────────────────────────────────────────────────────
    // Edit the first existing template
    await page.getByTestId(`edit-template-btn-${TEMPLATE_ID_1}`).click()

    await expect(page.getByTestId('template-form-dialog')).toBeVisible({ timeout: 5000 })

    // Verify form is pre-filled
    await expect(page.getByTestId('template-name-input')).toHaveValue('Appointment Confirmation')

    // Update the name
    await page.getByTestId('template-name-input').fill('Appointment Confirmation (Updated)')
    await page.getByTestId('template-form-save-btn').click()

    // Dialog should close
    await expect(page.getByTestId('template-form-dialog')).not.toBeVisible({ timeout: 5000 })

    // ── DELETE ─────────────────────────────────────────────────────────
    // Click delete on a template
    await page.getByTestId(`delete-template-btn-${TEMPLATE_ID_1}`).click()

    // Delete confirmation dialog should appear
    await expect(page.getByTestId('delete-template-dialog')).toBeVisible({ timeout: 5000 })

    // Confirm deletion
    await page.getByTestId('delete-template-confirm-btn').click()

    // Dialog should close and template should be removed
    await expect(page.getByTestId('delete-template-dialog')).not.toBeVisible({ timeout: 5000 })
    await expect(page.getByTestId(`template-row-${TEMPLATE_ID_1}`)).not.toBeVisible()
  })
})

test.describe('Admin Settings — Messaging Hours', () => {
  test('Messaging hours configuration', async ({ page }) => {
    await loginAsAdmin(page, '/en/settings/messaging/hours')
    await page.waitForSelector('[data-testid="messaging-hours-page"]', { timeout: 15000 })

    await expect(page.getByTestId('messaging-hours-page')).toBeVisible()

    // Hours table should be shown (one row per day of the week)
    await expect(page.getByTestId('hours-table')).toBeVisible()

    // Save button should be present
    const saveBtn = page.getByTestId('save-hours-btn')
    await expect(saveBtn).toBeVisible()

    // Click save to persist hours (even if unchanged)
    await saveBtn.click()

    // Button should not show an error — it may briefly disable
    // Wait for it to re-enable
    await expect(saveBtn).toBeEnabled({ timeout: 5000 })
  })
})

test.describe('Admin Settings — Stats Dashboard', () => {
  test('Stats dashboard displays metrics', async ({ page }) => {
    await loginAsAdmin(page, '/en/settings/messaging/stats')
    await page.waitForSelector('[data-testid="triage-stats-page"]', { timeout: 15000 })

    await expect(page.getByTestId('triage-stats-page')).toBeVisible()

    // Stats cards should be shown (total, open, avg response)
    await expect(page.getByTestId('stats-cards')).toBeVisible()

    // Category breakdown chart should be shown
    await expect(page.getByTestId('stats-category-chart')).toBeVisible()

    // Daily volume chart should be shown
    await expect(page.getByTestId('stats-volume-chart')).toBeVisible()
  })
})
