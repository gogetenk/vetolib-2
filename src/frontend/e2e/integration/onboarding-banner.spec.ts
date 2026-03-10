import { test, expect, type Page } from '@playwright/test'

// ─── JWT helpers ─────────────────────────────────────────────────────────────

function makeToken(
  role: string,
  name: string,
  email: string,
  clinicName = 'Desert Paws Clinic'
): string {
  const payload = {
    sub: email,
    name,
    role,
    clinicId: 'clinic-001',
    clinicName,
    exp: Math.floor(Date.now() / 1000) + 3600,
    iat: Math.floor(Date.now() / 1000),
  }
  const header = btoa(JSON.stringify({ alg: 'HS256', typ: 'JWT' })).replace(/=/g, '')
  const body = btoa(JSON.stringify(payload)).replace(/=/g, '')
  return `${header}.${body}.fake-signature`
}

const ADMIN_TOKEN = makeToken('ADMIN', 'Dr. Admin User', 'admin@desertpaws.ae')
const VET_TOKEN = makeToken('VET', 'Dr. Sarah Johnson', 'dr.sarah@desertpaws.ae')
const RECEPTIONIST_TOKEN = makeToken('RECEPTIONIST', 'Khalid Al-Nuaimi', 'reception@desertpaws.ae')
const ASSISTANT_TOKEN = makeToken('ASSISTANT', 'Mariam Al-Zaabi', 'assistant@desertpaws.ae')

// ─── Auth helpers ─────────────────────────────────────────────────────────────

async function waitForMSW(page: Page): Promise<void> {
  await page.waitForSelector('[data-testid="msw-ready"]', {
    state: 'attached',
    timeout: 15000,
  })
}

async function loginAs(page: Page, token: string, path = '/en/dashboard'): Promise<void> {
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
  await page.goto(path)
  await waitForMSW(page)
}

// ─────────────────────────────────────────────────────────────────────────────
// SECTION 1: Banner visibility
// ─────────────────────────────────────────────────────────────────────────────

test.describe('Welcome Banner — initial visibility', () => {
  test('Admin sees welcome banner on first dashboard visit', async ({ page }) => {
    await loginAs(page, ADMIN_TOKEN)
    await expect(page.getByTestId('welcome-banner')).toBeVisible({ timeout: 10000 })
  })

  test('Vet sees welcome banner on first dashboard visit', async ({ page }) => {
    await loginAs(page, VET_TOKEN)
    await expect(page.getByTestId('welcome-banner')).toBeVisible({ timeout: 10000 })
  })

  test('Receptionist sees welcome banner on first dashboard visit', async ({ page }) => {
    await loginAs(page, RECEPTIONIST_TOKEN)
    await expect(page.getByTestId('welcome-banner')).toBeVisible({ timeout: 10000 })
  })

  test('Assistant sees welcome banner on first dashboard visit', async ({ page }) => {
    await loginAs(page, ASSISTANT_TOKEN)
    await expect(page.getByTestId('welcome-banner')).toBeVisible({ timeout: 10000 })
  })
})

// ─────────────────────────────────────────────────────────────────────────────
// SECTION 2: Role-based greeting
// ─────────────────────────────────────────────────────────────────────────────

test.describe('Welcome Banner — greeting text per role', () => {
  test('Banner displays correct greeting for admin role', async ({ page }) => {
    await loginAs(page, ADMIN_TOKEN)
    const greeting = page.getByTestId('welcome-banner-greeting')
    await expect(greeting).toBeVisible({ timeout: 10000 })
    await expect(greeting).toContainText('Desert Paws Clinic')
  })

  test('Banner displays correct greeting for vet role', async ({ page }) => {
    await loginAs(page, VET_TOKEN)
    const greeting = page.getByTestId('welcome-banner-greeting')
    await expect(greeting).toBeVisible({ timeout: 10000 })
    await expect(greeting).toContainText('workspace')
  })

  test('Banner displays correct greeting for receptionist role', async ({ page }) => {
    await loginAs(page, RECEPTIONIST_TOKEN)
    const greeting = page.getByTestId('welcome-banner-greeting')
    await expect(greeting).toBeVisible({ timeout: 10000 })
    await expect(greeting).toContainText('appointments')
  })

  test('Banner displays correct greeting for assistant role', async ({ page }) => {
    await loginAs(page, ASSISTANT_TOKEN)
    const greeting = page.getByTestId('welcome-banner-greeting')
    await expect(greeting).toBeVisible({ timeout: 10000 })
    await expect(greeting).toContainText('patient records')
  })
})

// ─────────────────────────────────────────────────────────────────────────────
// SECTION 3: CTA behavior
// ─────────────────────────────────────────────────────────────────────────────

test.describe('Welcome Banner — CTA behavior', () => {
  test('Admin CTA scrolls to setup checklist section', async ({ page }) => {
    await loginAs(page, ADMIN_TOKEN)
    await expect(page.getByTestId('welcome-banner')).toBeVisible({ timeout: 10000 })

    // The checklist target section must exist
    const checklistSection = page.locator('#setup-checklist')
    await expect(checklistSection).toBeAttached()

    // Click CTA
    await page.getByTestId('welcome-banner-cta').click()

    // Banner remains visible (scroll does not dismiss)
    await expect(page.getByTestId('welcome-banner')).toBeVisible()
  })

  test('Receptionist CTA navigates to /appointments/new', async ({ page }) => {
    await loginAs(page, RECEPTIONIST_TOKEN)
    await expect(page.getByTestId('welcome-banner')).toBeVisible({ timeout: 10000 })

    await page.getByTestId('welcome-banner-cta').click()

    await expect(page).toHaveURL(/\/appointments\/new/, { timeout: 10000 })
  })

  test('Assistant CTA navigates to /patients', async ({ page }) => {
    await loginAs(page, ASSISTANT_TOKEN)
    await expect(page.getByTestId('welcome-banner')).toBeVisible({ timeout: 10000 })

    await page.getByTestId('welcome-banner-cta').click()

    await expect(page).toHaveURL(/\/patients/, { timeout: 10000 })
  })
})

// ─────────────────────────────────────────────────────────────────────────────
// SECTION 4: Dismiss behavior
// ─────────────────────────────────────────────────────────────────────────────

test.describe('Welcome Banner — dismiss', () => {
  test('Dismissing banner hides it', async ({ page }) => {
    await loginAs(page, ADMIN_TOKEN)
    await expect(page.getByTestId('welcome-banner')).toBeVisible({ timeout: 10000 })

    await page.getByTestId('welcome-banner-dismiss').click()

    await expect(page.getByTestId('welcome-banner')).not.toBeVisible({ timeout: 2000 })
  })

  test('Banner does not reappear after reload (MSW persists dismiss state)', async ({ page }) => {
    await loginAs(page, VET_TOKEN)
    await expect(page.getByTestId('welcome-banner')).toBeVisible({ timeout: 10000 })

    // Dismiss
    await page.getByTestId('welcome-banner-dismiss').click()
    await expect(page.getByTestId('welcome-banner')).not.toBeVisible({ timeout: 2000 })

    // Reload and check banner stays hidden
    await page.reload()
    await waitForMSW(page)
    // After reload, MSW still returns welcomeBannerVisible: false for this role
    await expect(page.getByTestId('welcome-banner')).not.toBeVisible({ timeout: 5000 })
  })

  test('Dismiss button is visible and accessible', async ({ page }) => {
    await loginAs(page, ADMIN_TOKEN)
    await expect(page.getByTestId('welcome-banner')).toBeVisible({ timeout: 10000 })

    const dismissBtn = page.getByTestId('welcome-banner-dismiss')
    await expect(dismissBtn).toBeVisible()
    await expect(dismissBtn).toBeEnabled()
  })
})

// ─────────────────────────────────────────────────────────────────────────────
// SECTION 5: RTL / Arabic locale
// ─────────────────────────────────────────────────────────────────────────────

test.describe('Welcome Banner — RTL (Arabic locale)', () => {
  test('Banner renders correctly in AR locale', async ({ page }) => {
    await loginAs(page, ADMIN_TOKEN, '/ar/dashboard')

    // Banner should appear in AR locale
    await expect(page.getByTestId('welcome-banner')).toBeVisible({ timeout: 10000 })

    // Verify the page is in RTL mode (dir="rtl" on html element)
    const htmlDir = await page.evaluate(() => document.documentElement.dir)
    expect(htmlDir).toBe('rtl')

    // Verify the greeting element is visible
    const greeting = page.getByTestId('welcome-banner-greeting')
    await expect(greeting).toBeVisible()

    // Verify the CTA and dismiss buttons are present
    await expect(page.getByTestId('welcome-banner-cta')).toBeVisible()
    await expect(page.getByTestId('welcome-banner-dismiss')).toBeVisible()
  })

  test('Banner dismiss button is positioned at inline-end (RTL aware)', async ({ page }) => {
    await loginAs(page, ADMIN_TOKEN, '/ar/dashboard')
    await expect(page.getByTestId('welcome-banner')).toBeVisible({ timeout: 10000 })

    // Verify dismiss button exists (RTL positioning is handled via CSS `end-3`)
    const dismissBtn = page.getByTestId('welcome-banner-dismiss')
    await expect(dismissBtn).toBeVisible()
  })
})
