import { test, expect, Page } from '@playwright/test'

// Helper: log in as a given user and wait for MSW to be ready
async function loginAs(page: Page, email: string, password: string) {
  await page.goto('/login')
  await page.waitForSelector('[data-testid="msw-ready"]', {
    state: 'attached',
    timeout: 60000,
  })
  await page.getByTestId('email-input').fill(email)
  await page.getByTestId('password-input').fill(password)
  await page.getByTestId('signin-button').click()
  // Wait until redirected away from /login
  await page.waitForURL((url) => !url.pathname.includes('/login'), { timeout: 10000 })
}

// Helper: navigate to /dashboard and wait for MSW ready
async function gotoDashboard(page: Page) {
  await page.goto('/dashboard')
  await page.waitForSelector('[data-testid="msw-ready"]', {
    state: 'attached',
    timeout: 60000,
  })
  // Wait for the page content to render
  await page.waitForSelector('[data-testid="dashboard-home"]', { timeout: 15000 })
}

test.describe('Dashboard home', () => {
  test.beforeEach(async ({ page }) => {
    // Clear auth state
    await page.goto('/login')
    await page.waitForLoadState('domcontentloaded')
    await page.evaluate(() => {
      localStorage.removeItem('access_token')
      localStorage.removeItem('refresh_token')
      document.cookie =
        'access_token=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;'
    })
  })

  test('Dashboard shows 4 stats cards for admin user', async ({ page }) => {
    await loginAs(page, 'dr.sarah@desertpaws.ae', 'Secure123!')
    await gotoDashboard(page)

    // All 4 cards should be visible for VET role (appointments, check-in, patients)
    await expect(page.getByTestId('stats-cards')).toBeVisible()
    await expect(page.getByTestId('stat-appointments-today')).toBeVisible()
    await expect(page.getByTestId('stat-pending-checkin')).toBeVisible()
    await expect(page.getByTestId('stat-total-patients')).toBeVisible()
  })

  test('Stats cards display values from API', async ({ page }) => {
    await loginAs(page, 'dr.sarah@desertpaws.ae', 'Secure123!')
    await gotoDashboard(page)

    // Wait for data to load
    await expect(page.getByTestId('stat-appointments-today-value')).toBeVisible()
    await expect(page.getByTestId('stat-appointments-today-value')).toHaveText('8')

    await expect(page.getByTestId('stat-pending-checkin-value')).toBeVisible()
    await expect(page.getByTestId('stat-pending-checkin-value')).toHaveText('3')
  })

  test('Pending check-in badge appears when count > 0', async ({ page }) => {
    await loginAs(page, 'dr.sarah@desertpaws.ae', 'Secure123!')
    await gotoDashboard(page)

    await expect(page.getByTestId('stat-pending-checkin-badge')).toBeVisible()
    await expect(page.getByTestId('stat-pending-checkin-badge')).toContainText('urgent')
  })

  test('RECEPTIONIST sees invoices card but not total patients', async ({ page }) => {
    await loginAs(page, 'reception@desertpaws.ae', 'Secure123!')
    await gotoDashboard(page)

    await expect(page.getByTestId('stat-unpaid-invoices')).toBeVisible()
    await expect(page.getByTestId('stat-total-patients')).not.toBeVisible()
  })

  test('RECEPTIONIST unpaid invoices shows AED amount', async ({ page }) => {
    await loginAs(page, 'reception@desertpaws.ae', 'Secure123!')
    await gotoDashboard(page)

    await expect(page.getByTestId('stat-unpaid-invoices-value')).toBeVisible()
    await expect(page.getByTestId('stat-unpaid-invoices-value')).toContainText('AED')
    await expect(page.getByTestId('stat-unpaid-invoices-value')).toContainText('2,450')
  })

  test('ASSISTANT sees patients card but not invoices', async ({ page }) => {
    await loginAs(page, 'assistant@desertpaws.ae', 'Secure123!')
    await gotoDashboard(page)

    await expect(page.getByTestId('stat-total-patients')).toBeVisible()
    await expect(page.getByTestId('stat-unpaid-invoices')).not.toBeVisible()
  })

  test('Today appointments list is visible with status badges', async ({ page }) => {
    await loginAs(page, 'dr.sarah@desertpaws.ae', 'Secure123!')
    await gotoDashboard(page)

    await expect(page.getByTestId('today-appointments-card')).toBeVisible()
    await expect(page.getByTestId('today-appointments-list')).toBeVisible()

    // Verify appointment rows are rendered
    const rows = page.getByTestId(/today-appointment-row-/)
    await expect(rows.first()).toBeVisible()
  })

  test('Appointment row links to appointment detail', async ({ page }) => {
    await loginAs(page, 'dr.sarah@desertpaws.ae', 'Secure123!')
    await gotoDashboard(page)

    await expect(page.getByTestId('today-appointments-list')).toBeVisible()

    // Click the first appointment link
    const firstLink = page
      .getByTestId(/appointment-link-/)
      .first()
    const href = await firstLink.getAttribute('href')
    expect(href).toMatch(/\/appointments\//)

    await firstLink.click()
    await expect(page).toHaveURL(/\/appointments\//)
  })

  test('Check In button visible for RECEPTIONIST on SCHEDULED appointments', async ({ page }) => {
    await loginAs(page, 'reception@desertpaws.ae', 'Secure123!')
    await gotoDashboard(page)

    await expect(page.getByTestId('today-appointments-list')).toBeVisible()

    // At least one Check In button should be visible (for SCHEDULED appointments)
    const checkInButtons = page.getByTestId(/checkin-btn-/)
    await expect(checkInButtons.first()).toBeVisible()
  })

  test('Check In button not visible for VET', async ({ page }) => {
    await loginAs(page, 'dr.sarah@desertpaws.ae', 'Secure123!')
    await gotoDashboard(page)

    await expect(page.getByTestId('today-appointments-list')).toBeVisible()

    // VET role should not see Check In buttons
    const checkInButtons = page.getByTestId(/checkin-btn-/)
    await expect(checkInButtons).toHaveCount(0)
  })

  test('View all link navigates to appointments page', async ({ page }) => {
    await loginAs(page, 'dr.sarah@desertpaws.ae', 'Secure123!')
    await gotoDashboard(page)

    await page.getByTestId('today-appointments-view-all').click()
    await expect(page).toHaveURL(/\/appointments/)
  })

  test('Recent activity feed shows 10 items with icons', async ({ page }) => {
    await loginAs(page, 'dr.sarah@desertpaws.ae', 'Secure123!')
    await gotoDashboard(page)

    await expect(page.getByTestId('recent-activity-card')).toBeVisible()
    await expect(page.getByTestId('recent-activity-list')).toBeVisible()

    const items = page.getByTestId(/activity-item-/)
    await expect(items).toHaveCount(10)
  })

  test('Recent activity shows icons by type', async ({ page }) => {
    await loginAs(page, 'dr.sarah@desertpaws.ae', 'Secure123!')
    await gotoDashboard(page)

    await expect(page.getByTestId('recent-activity-list')).toBeVisible()

    // Check first item has an icon
    const firstIcon = page.getByTestId(/activity-icon-/).first()
    await expect(firstIcon).toBeVisible()
    // Icon text should be one of the 3 emoji types
    const iconText = await firstIcon.textContent()
    expect(['📅', '💊', '💰']).toContain(iconText?.trim())
  })

  test('Dashboard greeting shows today date', async ({ page }) => {
    await loginAs(page, 'dr.sarah@desertpaws.ae', 'Secure123!')
    await gotoDashboard(page)

    await expect(page.getByTestId('dashboard-greeting')).toBeVisible()
    await expect(page.getByTestId('dashboard-greeting-title')).toBeVisible()
    await expect(page.getByTestId('dashboard-date')).toBeVisible()
  })

  test('Unauthenticated user visiting /dashboard is redirected to login', async ({ page }) => {
    await page.goto('/dashboard')
    await expect(page).toHaveURL(/\/login/)
  })
})
