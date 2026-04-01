import { test, expect, type Page } from '@playwright/test'

// Minimal JWT tokens for testing — role encoded in payload
function makeToken(role: string, name: string, email: string): string {
  const payload = {
    sub: email,
    name,
    role,
    clinic_id: 'clinic-001',
    clinic_name: 'Desert Paws Clinic',
    exp: Math.floor(Date.now() / 1000) + 3600,
  }
  const header = btoa(JSON.stringify({ alg: 'HS256', typ: 'JWT' })).replace(/=/g, '')
  const body = btoa(JSON.stringify(payload)).replace(/=/g, '')
  return `${header}.${body}.fake-signature`
}

const VET_TOKEN = makeToken('VET', 'Dr. Sarah Johnson', 'dr.sarah@desertpaws.ae')
const RECEPTIONIST_TOKEN = makeToken('RECEPTIONIST', 'Khalid Al-Nuaimi', 'reception@desertpaws.ae')
const ASSISTANT_TOKEN = makeToken('ASSISTANT', 'Mariam Al-Zaabi', 'assistant@desertpaws.ae')

const MAX_PATIENT_ID = 'pat-0000-0000-0000-000000000001'
// const _LUNA_PATIENT_ID = 'pat-0000-0000-0000-000000000002'
const CAMEL_PATIENT_ID = 'pat-0000-0000-0000-000000000005'

/** Wait for MSW service worker to be active before asserting data-driven UI. */
async function waitForMSW(page: Page): Promise<void> {
  await page.waitForSelector('[data-testid="msw-ready"]', { state: 'attached', timeout: 15000 })
}

async function loginAs(page: Page, token: string, targetPath = '/patients'): Promise<void> {
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
// Patients List
// ────────────────────────────────────────────────────────────────────────────
test.describe('Patients list — VET', () => {
  test.beforeEach(async ({ page }) => {
    await loginAs(page, VET_TOKEN, '/patients')
    await page.waitForSelector('[data-testid="patients-table"]', { timeout: 15000 })
  })

  test('shows Add Patient button for VET', async ({ page }) => {
    await expect(page.getByTestId('add-patient-btn')).toBeVisible()
  })

  test('shows 5 patients including Camel (Zayed)', async ({ page }) => {
    const cards = page.locator('[data-testid^="patient-card-"]')
    await expect(cards).toHaveCount(5)
    await expect(page.getByTestId(`patient-card-${CAMEL_PATIENT_ID}`)).toBeVisible()
  })

  test('search filters by name live', async ({ page }) => {
    const searchInput = page.getByTestId('search-input')
    await searchInput.fill('Zayed')
    await page.waitForTimeout(400) // debounce
    await page.waitForLoadState('networkidle')

    const cards = page.locator('[data-testid^="patient-card-"]')
    await expect(cards).toHaveCount(1)
    await expect(page.getByTestId(`patient-name-${CAMEL_PATIENT_ID}`)).toContainText('Zayed')
  })

  test('search filters by owner name live', async ({ page }) => {
    const searchInput = page.getByTestId('search-input')
    await searchInput.fill('Khalid')
    await page.waitForTimeout(400) // debounce
    await page.waitForLoadState('networkidle')

    const cards = page.locator('[data-testid^="patient-card-"]')
    await expect(cards.first()).toBeVisible()
    await expect(page.getByTestId(`patient-owner-${CAMEL_PATIENT_ID}`)).toContainText('Khalid Al-Mazrouei')
  })

  test('clicking View Record navigates to patient detail', async ({ page }) => {
    await page.getByTestId(`view-record-btn-${MAX_PATIENT_ID}`).click()
    await expect(page).toHaveURL(new RegExp(`/patients/${MAX_PATIENT_ID}`))
  })
})

test.describe('Patients list — RBAC read-only', () => {
  test('RECEPTIONIST does not see Add Patient button', async ({ page }) => {
    await loginAs(page, RECEPTIONIST_TOKEN, '/patients')
    await page.waitForSelector('[data-testid="patients-page"]', { timeout: 15000 })
    await expect(page.getByTestId('add-patient-btn')).not.toBeVisible()
  })

  test('ASSISTANT does not see Add Patient button', async ({ page }) => {
    await loginAs(page, ASSISTANT_TOKEN, '/patients')
    await page.waitForSelector('[data-testid="patients-page"]', { timeout: 15000 })
    await expect(page.getByTestId('add-patient-btn')).not.toBeVisible()
  })
})

// ────────────────────────────────────────────────────────────────────────────
// New Patient Form
// ────────────────────────────────────────────────────────────────────────────
test.describe('New Patient form', () => {
  test.beforeEach(async ({ page }) => {
    await loginAs(page, VET_TOKEN, '/patients/new')
    await page.waitForSelector('[data-testid="new-patient-page"]', { timeout: 15000 })
    await page.waitForSelector('[data-testid="patient-form"]', { timeout: 15000 })
  })

  test('shows the new patient form', async ({ page }) => {
    await expect(page.getByTestId('new-patient-page')).toBeVisible()
    await expect(page.getByTestId('patient-form')).toBeVisible()
    await expect(page.getByTestId('new-patient-title')).toContainText('New Patient')
  })

  test('shows species select with Camel option', async ({ page }) => {
    await page.getByTestId('select-species-trigger').click()
    await expect(page.getByTestId('species-option-camel')).toBeVisible()
    await expect(page.getByTestId('species-option-dog')).toBeVisible()
    await expect(page.getByTestId('species-option-cat')).toBeVisible()
  })

  test('creates a dog patient and redirects to detail', async ({ page }) => {
    await page.getByTestId('input-patient-name').fill('Rocky')
    await page.getByTestId('select-species-trigger').click()
    await page.getByTestId('species-option-dog').click()
    await page.getByTestId('input-breed').fill('German Shepherd')
    await page.getByTestId('input-date-of-birth').fill('2022-04-10')
    await page.getByTestId('select-gender-trigger').click()
    await page.getByTestId('gender-option-male').click()
    await page.getByTestId('input-owner-name').fill('Omar Al-Suwaidi')
    await page.getByTestId('input-owner-phone').fill('+971 50 111 2222')

    await page.getByTestId('btn-save-patient').click()

    // Should redirect to patient detail page
    await expect(page).toHaveURL(/\/patients\/[a-z0-9-]+$/)
    await expect(page).not.toHaveURL(/\/patients\/new/)
  })

  test('creates a Camel patient successfully', async ({ page }) => {
    await page.getByTestId('input-patient-name').fill('Noor')
    await page.getByTestId('select-species-trigger').click()
    await page.getByTestId('species-option-camel').click()
    await page.getByTestId('input-breed').fill('Dromedary')
    await page.getByTestId('input-date-of-birth').fill('2020-01-15')
    await page.getByTestId('select-gender-trigger').click()
    await page.getByTestId('gender-option-female').click()
    await page.getByTestId('input-owner-name').fill('Hamdan Al-Rashidi')
    await page.getByTestId('input-owner-phone').fill('+971 56 333 4444')

    await page.getByTestId('btn-save-patient').click()

    // Should redirect to patient detail page
    await expect(page).toHaveURL(/\/patients\/[a-z0-9-]+$/)
    await expect(page).not.toHaveURL(/\/patients\/new/)
  })

  test('validates required fields on submit', async ({ page }) => {
    await page.getByTestId('btn-save-patient').click()

    // Should show validation errors for required fields
    await expect(page.getByTestId('error-patient-name')).toBeVisible()
    await expect(page.getByTestId('error-species')).toBeVisible()
    await expect(page.getByTestId('error-date-of-birth')).toBeVisible()
    await expect(page.getByTestId('error-owner-name')).toBeVisible()
    await expect(page.getByTestId('error-owner-phone')).toBeVisible()
  })

  test('validates UAE phone format', async ({ page }) => {
    await page.getByTestId('input-owner-phone').fill('0501234567')
    await page.getByTestId('btn-save-patient').click()

    await expect(page.getByTestId('error-owner-phone')).toBeVisible()
    await expect(page.getByTestId('error-owner-phone')).toContainText('UAE format')
  })

  test('does not allow future date of birth', async ({ page }) => {
    const futureDate = new Date()
    futureDate.setFullYear(futureDate.getFullYear() + 1)
    const futureDateStr = futureDate.toISOString().split('T')[0]

    await page.getByTestId('input-patient-name').fill('Test')
    await page.getByTestId('input-date-of-birth').fill(futureDateStr)
    await page.getByTestId('btn-save-patient').click()

    await expect(page.getByTestId('error-date-of-birth')).toBeVisible()
    await expect(page.getByTestId('error-date-of-birth')).toContainText('future')
  })

  test('Cancel button goes back to patients list', async ({ page }) => {
    await page.getByTestId('btn-cancel-patient').click()
    await expect(page).toHaveURL(/\/patients$/)
  })
})

// ────────────────────────────────────────────────────────────────────────────
// Patient Detail — Age and Edit
// ────────────────────────────────────────────────────────────────────────────
test.describe('Patient detail — standalone enhancements', () => {
  test.beforeEach(async ({ page }) => {
    await loginAs(page, VET_TOKEN, `/patients/${MAX_PATIENT_ID}`)
    await page.waitForSelector('[data-testid="patient-detail-page"]', { timeout: 20000 })
    await page.waitForSelector('[data-testid="patient-detail-name"]', { timeout: 20000 })
  })

  test('shows calculated age (not just ageYears)', async ({ page }) => {
    const ageEl = page.getByTestId('patient-detail-age')
    await expect(ageEl).toBeVisible()
    // Max born 2019-03-15, so should show years and months
    const ageText = await ageEl.textContent()
    expect(ageText).toMatch(/\d+ year/)
  })

  test('shows species icon in header', async ({ page }) => {
    await expect(page.getByTestId('patient-species-avatar')).toBeVisible()
  })

  test('VET sees Edit button on patient detail', async ({ page }) => {
    await expect(page.getByTestId('edit-patient-btn')).toBeVisible()
  })

  test('Edit button opens a drawer with the patient form', async ({ page }) => {
    await page.getByTestId('edit-patient-btn').click()
    await expect(page.getByTestId('patient-form')).toBeVisible({ timeout: 5000 })
    // Form should be pre-filled with patient data
    await expect(page.getByTestId('input-patient-name')).toHaveValue('Max')
  })

  test('can update patient name from the edit drawer', async ({ page }) => {
    await page.getByTestId('edit-patient-btn').click()
    await expect(page.getByTestId('patient-form')).toBeVisible({ timeout: 5000 })

    // Clear and retype the name
    await page.getByTestId('input-patient-name').fill('Max Updated')
    await page.getByTestId('btn-save-patient').click()

    // Drawer should close and patient name should update
    await expect(page.getByTestId('patient-form')).not.toBeVisible({ timeout: 5000 })
    await expect(page.getByTestId('patient-detail-name')).toContainText('Max Updated')
  })
})

test.describe('Patient detail — RBAC read-only', () => {
  test('RECEPTIONIST does not see Edit button', async ({ page }) => {
    await loginAs(page, RECEPTIONIST_TOKEN, `/patients/${MAX_PATIENT_ID}`)
    await page.waitForSelector('[data-testid="patient-detail-page"]', { timeout: 20000 })
    await expect(page.getByTestId('edit-patient-btn')).not.toBeVisible()
  })

  test('ASSISTANT does not see Edit button', async ({ page }) => {
    await loginAs(page, ASSISTANT_TOKEN, `/patients/${MAX_PATIENT_ID}`)
    await page.waitForSelector('[data-testid="patient-detail-page"]', { timeout: 20000 })
    await expect(page.getByTestId('edit-patient-btn')).not.toBeVisible()
  })
})

// ────────────────────────────────────────────────────────────────────────────
// New patient creation end-to-end (redirect to detail)
// ────────────────────────────────────────────────────────────────────────────
test.describe('New patient creation flow', () => {
  test('after creation, patient detail page shows the new patient data', async ({ page }) => {
    await loginAs(page, VET_TOKEN, '/patients/new')
    await page.waitForSelector('[data-testid="patient-form"]', { timeout: 15000 })

    await page.getByTestId('input-patient-name').fill('Layla')
    await page.getByTestId('select-species-trigger').click()
    await page.getByTestId('species-option-camel').click()
    await page.getByTestId('input-date-of-birth').fill('2019-06-01')
    await page.getByTestId('select-gender-trigger').click()
    await page.getByTestId('gender-option-female').click()
    await page.getByTestId('input-owner-name').fill('Fatima Al-Shamsi')
    await page.getByTestId('input-owner-phone').fill('+971 52 555 6666')
    await page.getByTestId('btn-save-patient').click()

    // Wait for redirect to patient detail
    await expect(page).toHaveURL(/\/patients\/[a-z0-9-]+$/)
    await page.waitForSelector('[data-testid="patient-detail-page"]', { timeout: 10000 })

    // The patient detail page should show the new patient
    await expect(page.getByTestId('patient-detail-name')).toContainText('Layla')
  })
})
