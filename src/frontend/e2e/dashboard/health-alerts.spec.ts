import { test, expect, Page } from '@playwright/test'

async function loginAs(page: Page, email: string, password: string) {
  await page.goto('/login')
  await page.waitForSelector('[data-testid="msw-ready"]', {
    state: 'attached',
    timeout: 60000,
  })
  await page.getByTestId('email-input').fill(email)
  await page.getByTestId('password-input').fill(password)
  await page.getByTestId('signin-button').click()
  await page.waitForURL((url) => !url.pathname.includes('/login'), { timeout: 10000 })
}

async function gotoDashboard(page: Page) {
  await page.goto('/dashboard')
  await page.waitForSelector('[data-testid="msw-ready"]', {
    state: 'attached',
    timeout: 60000,
  })
  await page.waitForSelector('[data-testid="dashboard-home"]', { timeout: 15000 })
}

test.describe('Health Alerts on Dashboard', () => {
  test.beforeEach(async ({ page }) => {
    await page.goto('/login')
    await page.waitForLoadState('domcontentloaded')
    await page.evaluate(() => {
      localStorage.removeItem('access_token')
      localStorage.removeItem('refresh_token')
      document.cookie =
        'access_token=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;'
    })
  })

  test('Vet sees health alerts panel on dashboard with alert count', async ({ page }) => {
    await loginAs(page, 'dr.sarah@desertpaws.ae', 'Secure123!')
    await gotoDashboard(page)

    // Health alerts panel should be visible
    await expect(page.getByTestId('health-alerts-panel')).toBeVisible()

    // Badge with count should be visible
    await expect(page.getByTestId('health-alerts-count-badge')).toBeVisible()
    const countText = await page.getByTestId('health-alerts-count-badge').textContent()
    const count = parseInt(countText?.trim() ?? '0')
    expect(count).toBeGreaterThan(0)
  })

  test('Alert panel is grouped by severity (High, Medium, Low)', async ({ page }) => {
    await loginAs(page, 'dr.sarah@desertpaws.ae', 'Secure123!')
    await gotoDashboard(page)

    await expect(page.getByTestId('health-alerts-panel')).toBeVisible()

    // High severity group should appear
    await expect(page.getByTestId('health-alerts-group-high')).toBeVisible()

    // Medium severity group should appear
    await expect(page.getByTestId('health-alerts-group-medium')).toBeVisible()

    // Low severity group should appear
    await expect(page.getByTestId('health-alerts-group-low')).toBeVisible()
  })

  test('Each alert shows patient name, breed, title, and description', async ({ page }) => {
    await loginAs(page, 'dr.sarah@desertpaws.ae', 'Secure123!')
    await gotoDashboard(page)

    await expect(page.getByTestId('health-alerts-panel')).toBeVisible()

    // Check first alert has all required fields
    const firstAlertId = 'ha-0001-0000-0000-000000000001'
    await expect(page.getByTestId(`alert-patient-name-${firstAlertId}`)).toBeVisible()
    await expect(page.getByTestId(`alert-patient-name-${firstAlertId}`)).toHaveText('Simba')
    await expect(page.getByTestId(`alert-title-${firstAlertId}`)).toBeVisible()
    await expect(page.getByTestId(`alert-title-${firstAlertId}`)).toContainText('Rabies Vaccination')
    await expect(page.getByTestId(`alert-description-${firstAlertId}`)).toBeVisible()
  })

  test('Vet can dismiss an alert with a reason', async ({ page }) => {
    await loginAs(page, 'dr.sarah@desertpaws.ae', 'Secure123!')
    await gotoDashboard(page)

    await expect(page.getByTestId('health-alerts-panel')).toBeVisible()

    // Get initial count
    const initialCount = await page.getByTestId('health-alerts-count-badge').textContent()

    // Click dismiss on first alert
    const firstAlertId = 'ha-0001-0000-0000-000000000001'
    await page.getByTestId(`alert-dismiss-btn-${firstAlertId}`).click()

    // Dialog should open
    await expect(page.getByTestId('dismiss-alert-dialog')).toBeVisible()
    await expect(page.getByTestId('dismiss-reason-input')).toBeVisible()

    // Confirm button should be disabled without reason
    await expect(page.getByTestId('dismiss-confirm-btn')).toBeDisabled()

    // Type a reason
    await page.getByTestId('dismiss-reason-input').fill('Owner confirmed vaccination done elsewhere')

    // Confirm button should now be enabled
    await expect(page.getByTestId('dismiss-confirm-btn')).toBeEnabled()

    // Click confirm
    await page.getByTestId('dismiss-confirm-btn').click()

    // Dialog should close
    await expect(page.getByTestId('dismiss-alert-dialog')).not.toBeVisible()

    // Alert count should decrease
    const newCount = await page.getByTestId('health-alerts-count-badge').textContent()
    expect(parseInt(newCount?.trim() ?? '0')).toBeLessThan(parseInt(initialCount?.trim() ?? '0'))
  })

  test('Vet can acknowledge an alert', async ({ page }) => {
    await loginAs(page, 'dr.sarah@desertpaws.ae', 'Secure123!')
    await gotoDashboard(page)

    await expect(page.getByTestId('health-alerts-panel')).toBeVisible()

    // Click acknowledge on the second alert
    const alertId = 'ha-0001-0000-0000-000000000002'
    await page.getByTestId(`alert-acknowledge-btn-${alertId}`).click()

    // After acknowledging, the acknowledge button should disappear and a badge should show
    await expect(page.getByTestId(`alert-acknowledged-badge-${alertId}`)).toBeVisible()
    await expect(page.getByTestId(`alert-acknowledge-btn-${alertId}`)).not.toBeVisible()
  })

  test('Schedule Appointment button navigates to appointment creation', async ({ page }) => {
    await loginAs(page, 'dr.sarah@desertpaws.ae', 'Secure123!')
    await gotoDashboard(page)

    await expect(page.getByTestId('health-alerts-panel')).toBeVisible()

    // Click schedule appointment on first alert
    const firstAlertId = 'ha-0001-0000-0000-000000000001'
    await page.getByTestId(`alert-schedule-btn-${firstAlertId}`).click()

    // Should navigate to appointment creation page with pre-filled data
    await expect(page).toHaveURL(/\/appointments\/new/)
  })

  test('All action buttons have data-testid attributes', async ({ page }) => {
    await loginAs(page, 'dr.sarah@desertpaws.ae', 'Secure123!')
    await gotoDashboard(page)

    await expect(page.getByTestId('health-alerts-panel')).toBeVisible()

    // Verify all buttons have data-testid
    const scheduleButtons = page.getByTestId(/alert-schedule-btn-/)
    const acknowledgeButtons = page.getByTestId(/alert-acknowledge-btn-/)
    const dismissButtons = page.getByTestId(/alert-dismiss-btn-/)

    expect(await scheduleButtons.count()).toBeGreaterThan(0)
    expect(await acknowledgeButtons.count()).toBeGreaterThan(0)
    expect(await dismissButtons.count()).toBeGreaterThan(0)
  })
})
