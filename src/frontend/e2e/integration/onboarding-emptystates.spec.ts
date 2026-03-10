import { test, expect, type Page } from '@playwright/test'

// ─── JWT helpers ─────────────────────────────────────────────────────────────

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

const ADMIN_TOKEN = makeToken('ADMIN', 'Omar Al-Rashid', 'admin@desertpaws.ae')
const VET_TOKEN = makeToken('VET', 'Dr. Sarah Johnson', 'dr.sarah@desertpaws.ae')

// ─── Helpers ─────────────────────────────────────────────────────────────────

async function waitForMSW(page: Page): Promise<void> {
  await page.waitForSelector('[data-testid="msw-ready"]', { state: 'attached', timeout: 15000 })
}

async function loginAs(page: Page, token: string, targetPath: string): Promise<void> {
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

// Override an API endpoint to return empty paged result
async function mockEmptyPatients(page: Page): Promise<void> {
  await page.route('**/api/patients**', (route) => {
    route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({ items: [], totalCount: 0, page: 1, pageSize: 20 }),
    })
  })
}

async function mockEmptyInvoices(page: Page): Promise<void> {
  await page.route('**/api/invoices**', (route) => {
    const url = route.request().url()
    // Only mock the list endpoint, not individual invoice endpoints
    if (!url.match(/\/api\/invoices\/[^?]+$/)) {
      route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({ items: [], totalCount: 0, page: 1, pageSize: 10 }),
      })
    } else {
      route.continue()
    }
  })
}

async function mockSingleUser(page: Page, email: string): Promise<void> {
  await page.route('**/api/users', (route) => {
    route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify([
        {
          id: 'u-admin-001',
          email,
          fullName: 'Omar Al-Rashid',
          role: 'ADMIN',
          isActive: true,
        },
      ]),
    })
  })
}

async function mockEmptyTodayAppointments(page: Page): Promise<void> {
  await page.route('**/api/appointments/today**', (route) => {
    route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify([]),
    })
  })
}

// ─── Tests ───────────────────────────────────────────────────────────────────

test.describe('Enriched Empty States', () => {
  test('Patients page shows enriched empty state when no patients', async ({ page }) => {
    await mockEmptyPatients(page)
    await loginAs(page, VET_TOKEN, '/en/patients')
    await page.waitForSelector('[data-testid="patients-page"]', { timeout: 15000 })

    const emptyState = page.getByTestId('empty-state-patients')
    await expect(emptyState).toBeVisible()
    await expect(emptyState).toContainText('Your patient list is empty')
    await expect(emptyState).toContainText('Add your first patient')
  })

  test('Patients empty state CTA navigates to /patients/new', async ({ page }) => {
    await mockEmptyPatients(page)
    await loginAs(page, VET_TOKEN, '/en/patients')
    await page.waitForSelector('[data-testid="empty-state-patients"]', { timeout: 15000 })

    const cta = page.getByTestId('empty-state-cta-patients')
    await expect(cta).toBeVisible()
    await cta.click()
    await expect(page).toHaveURL(/\/patients\/new/)
  })

  test('Patients empty state Import CSV CTA opens import dialog', async ({ page }) => {
    await mockEmptyPatients(page)
    await loginAs(page, VET_TOKEN, '/en/patients')
    await page.waitForSelector('[data-testid="empty-state-patients"]', { timeout: 15000 })

    const importCta = page.getByTestId('empty-state-cta-import')
    await expect(importCta).toBeVisible()
    await importCta.click()
    // CsvImportDialog should open — check for dialog element
    await expect(page.locator('[role="dialog"]')).toBeVisible()
  })

  test('Patients empty state shows tip', async ({ page }) => {
    await mockEmptyPatients(page)
    await loginAs(page, VET_TOKEN, '/en/patients')
    await page.waitForSelector('[data-testid="empty-state-patients"]', { timeout: 15000 })

    const tip = page.getByTestId('empty-state-tip-patients')
    await expect(tip).toBeVisible()
    await expect(tip).toContainText('Tip:')
  })

  test('Patients empty state disappears when data is present', async ({ page }) => {
    // No route override — MSW returns real patients data
    await loginAs(page, VET_TOKEN, '/en/patients')
    await page.waitForSelector('[data-testid="patients-page"]', { timeout: 15000 })

    // Wait for loading to finish
    await page.waitForSelector('[data-testid="patients-loading"]', { state: 'detached', timeout: 10000 }).catch(() => {})

    const emptyState = page.getByTestId('empty-state-patients')
    await expect(emptyState).not.toBeVisible()
    const table = page.getByTestId('patients-table')
    await expect(table).toBeVisible()
  })

  test('Billing page shows enriched empty state when no invoices', async ({ page }) => {
    await mockEmptyInvoices(page)
    await loginAs(page, ADMIN_TOKEN, '/en/billing')
    await page.waitForSelector('[data-testid="billing-page"]', { timeout: 15000 })

    const emptyState = page.getByTestId('empty-state-billing')
    await expect(emptyState).toBeVisible()
    await expect(emptyState).toContainText('No invoices yet')
    await expect(emptyState).toContainText('VAT 5%')
  })

  test('Billing empty state CTA navigates to /billing/new', async ({ page }) => {
    await mockEmptyInvoices(page)
    await loginAs(page, ADMIN_TOKEN, '/en/billing')
    await page.waitForSelector('[data-testid="empty-state-billing"]', { timeout: 15000 })

    const cta = page.getByTestId('empty-state-cta-billing')
    await expect(cta).toBeVisible()
    await cta.click()
    await expect(page).toHaveURL(/\/billing\/new/)
  })

  test('Billing empty state shows tip', async ({ page }) => {
    await mockEmptyInvoices(page)
    await loginAs(page, ADMIN_TOKEN, '/en/billing')
    await page.waitForSelector('[data-testid="empty-state-billing"]', { timeout: 15000 })

    const tip = page.getByTestId('empty-state-tip-billing')
    await expect(tip).toBeVisible()
    await expect(tip).toContainText('Tip:')
  })

  test('Team page shows enriched empty state when only admin', async ({ page }) => {
    await mockSingleUser(page, 'admin@desertpaws.ae')
    await loginAs(page, ADMIN_TOKEN, '/en/settings/team')
    await page.waitForSelector('[data-testid="team-page"]', { timeout: 15000 })

    const emptyState = page.getByTestId('empty-state-team')
    await expect(emptyState).toBeVisible()
    await expect(emptyState).toContainText("You're the only team member")
  })

  test('Team empty state Invite Member CTA opens invite dialog', async ({ page }) => {
    await mockSingleUser(page, 'admin@desertpaws.ae')
    await loginAs(page, ADMIN_TOKEN, '/en/settings/team')
    await page.waitForSelector('[data-testid="empty-state-team"]', { timeout: 15000 })

    const cta = page.getByTestId('empty-state-cta-team')
    await expect(cta).toBeVisible()
    await cta.click()
    await expect(page.locator('[role="dialog"]')).toBeVisible()
  })

  test('Team empty state shows tip', async ({ page }) => {
    await mockSingleUser(page, 'admin@desertpaws.ae')
    await loginAs(page, ADMIN_TOKEN, '/en/settings/team')
    await page.waitForSelector('[data-testid="empty-state-team"]', { timeout: 15000 })

    const tip = page.getByTestId('empty-state-tip-team')
    await expect(tip).toBeVisible()
    await expect(tip).toContainText('Tip:')
  })

  test('Dashboard today section shows inline empty state when no appointments today', async ({ page }) => {
    await mockEmptyTodayAppointments(page)
    await loginAs(page, ADMIN_TOKEN, '/en/dashboard')
    await page.waitForSelector('[data-testid="today-appointments-card"]', { timeout: 15000 })

    const emptyState = page.getByTestId('empty-state-dashboard-today')
    await expect(emptyState).toBeVisible()
    await expect(emptyState).toContainText('No appointments scheduled for today.')
  })

  test('Dashboard today empty state CTA links to /appointments', async ({ page }) => {
    await mockEmptyTodayAppointments(page)
    await loginAs(page, ADMIN_TOKEN, '/en/dashboard')
    await page.waitForSelector('[data-testid="empty-state-dashboard-today"]', { timeout: 15000 })

    const cta = page.getByTestId('empty-state-cta-dashboard-today')
    await expect(cta).toBeVisible()
    await expect(cta).toHaveAttribute('href', /\/appointments/)
  })

  test('Empty states render correctly in Arabic (RTL)', async ({ page }) => {
    await mockEmptyPatients(page)
    await loginAs(page, VET_TOKEN, '/ar/patients')
    await page.waitForSelector('[data-testid="patients-page"]', { timeout: 15000 })

    const emptyState = page.getByTestId('empty-state-patients')
    await expect(emptyState).toBeVisible()
    // Arabic text for "قائمة المرضى فارغة"
    await expect(emptyState).toContainText('قائمة المرضى فارغة')
    // RTL direction on html element
    const htmlDir = await page.locator('html').getAttribute('dir')
    expect(htmlDir).toBe('rtl')
  })
})
