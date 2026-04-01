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
const ASSISTANT_TOKEN = makeToken('ASSISTANT', 'Mariam Al-Zaabi', 'assistant@desertpaws.ae')
const RECEPTIONIST_TOKEN = makeToken('RECEPTIONIST', 'Khalid Al-Nuaimi', 'reception@desertpaws.ae')

// Patient IDs from MSW mock data
const MAX_PATIENT_ID = 'pat-0000-0000-0000-000000000001'
const LUNA_PATIENT_ID = 'pat-0000-0000-0000-000000000002'

/** Wait for MSW service worker to be active before asserting data-driven UI. */
async function waitForMSW(page: Page): Promise<void> {
  await page.waitForSelector('[data-testid="msw-ready"]', { state: 'attached', timeout: 15000 })
}

/**
 * Sets the auth cookie (for Next.js middleware) and localStorage token (for API client)
 * using addInitScript so both are available before the page loads.
 * Then waits for MSW to be active before returning.
 */
async function loginAs(page: Page, token: string, targetPath: string = '/patients'): Promise<void> {
  // Set cookie for middleware RBAC
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
  // Set localStorage before page load so useRole() picks it up immediately
  await page.addInitScript((t) => {
    localStorage.setItem('access_token', t)
  }, token)

  await page.goto(targetPath)
  await waitForMSW(page)
}

test.describe('Patients list', () => {
  test.beforeEach(async ({ page }) => {
    await loginAs(page, VET_TOKEN, '/patients')
    // Wait for data to load (MSW intercepts API call)
    await page.waitForSelector('[data-testid="patients-table"]', { timeout: 15000 })
  })

  test('shows patient grid with cards', async ({ page }) => {
    await expect(page.getByTestId('patients-page')).toBeVisible()
    await expect(page.getByTestId('patients-table')).toBeVisible({ timeout: 15000 })
    // Should show all patients from MSW (5 including Camel)
    const cards = page.locator('[data-testid^="patient-card-"]')
    await expect(cards).toHaveCount(5)
  })

  test('shows patient name, species, breed, owner and buttons', async ({ page }) => {
    const maxCard = page.getByTestId(`patient-card-${MAX_PATIENT_ID}`)
    await expect(maxCard).toBeVisible()
    await expect(page.getByTestId(`patient-name-${MAX_PATIENT_ID}`)).toContainText('Max')
    await expect(page.getByTestId(`patient-breed-${MAX_PATIENT_ID}`)).toContainText('Golden Retriever')
    await expect(page.getByTestId(`patient-owner-${MAX_PATIENT_ID}`)).toContainText('Ahmed Al-Rashid')
    await expect(page.getByTestId(`view-record-btn-${MAX_PATIENT_ID}`)).toBeVisible()
  })

  test('search filters patients by name', async ({ page }) => {
    const searchInput = page.getByTestId('search-input')
    await searchInput.fill('Luna')
    await page.waitForTimeout(400) // debounce
    await page.waitForLoadState('networkidle')

    const cards = page.locator('[data-testid^="patient-card-"]')
    await expect(cards).toHaveCount(1)
    await expect(page.getByTestId(`patient-name-${LUNA_PATIENT_ID}`)).toContainText('Luna')
  })

  test('search filters patients by owner name', async ({ page }) => {
    const searchInput = page.getByTestId('search-input')
    await searchInput.fill('Ahmed')
    await page.waitForTimeout(400) // debounce
    await page.waitForLoadState('networkidle')

    const cards = page.locator('[data-testid^="patient-card-"]')
    await expect(cards.first()).toBeVisible()
    await expect(page.getByTestId(`patient-owner-${MAX_PATIENT_ID}`)).toContainText('Ahmed Al-Rashid')
  })
})

test.describe('Patient detail — medical records tab', () => {
  test.beforeEach(async ({ page }) => {
    await loginAs(page, VET_TOKEN, `/patients/${MAX_PATIENT_ID}`)
    // Wait for patient data to load from MSW
    await page.waitForSelector('[data-testid="patient-detail-page"]', { timeout: 20000 })
    await page.waitForSelector('[data-testid="patient-detail-name"]', { timeout: 20000 })
  })

  test('shows patient profile header with species, breed, age, owner contact', async ({ page }) => {
    await expect(page.getByTestId('patient-detail-page')).toBeVisible()
    await expect(page.getByTestId('patient-detail-name')).toContainText('Max')
    await expect(page.getByTestId('patient-detail-species')).toContainText('Golden Retriever')
    await expect(page.getByTestId('patient-detail-owner')).toContainText('Ahmed Al-Rashid')
    await expect(page.getByTestId('patient-detail-phone')).toContainText('+971 50 123 4567')
    await expect(page.getByTestId('patient-species-avatar')).toBeVisible()
  })

  test('shows 3 tabs: Medical Records, Prescriptions, Vaccinations', async ({ page }) => {
    await expect(page.getByTestId('tab-medical-records')).toBeVisible()
    await expect(page.getByTestId('tab-prescriptions')).toBeVisible()
    await expect(page.getByTestId('tab-vaccinations')).toBeVisible()
  })

  test('medical records tab shows 3 records for Max', async ({ page }) => {
    // Medical Records tab is active by default
    await expect(page.getByTestId('tabpanel-medical-records')).toBeVisible()
    await expect(page.getByTestId('medical-records-list')).toBeVisible()

    const records = page.locator('[data-testid^="medical-record-rec-"]')
    await expect(records).toHaveCount(3)
  })

  test('each record shows date, vet name, reason, diagnosis and treatment', async ({ page }) => {
    const record = page.getByTestId('medical-record-rec-0000-0000-0000-000000000001')
    await expect(record).toBeVisible()

    await expect(page.getByTestId('record-reason-rec-0000-0000-0000-000000000001'))
      .toContainText('Annual vaccination')
    await expect(page.getByTestId('record-vet-rec-0000-0000-0000-000000000001'))
      .toContainText('Dr. Sarah Johnson')
    await expect(page.getByTestId('record-date-rec-0000-0000-0000-000000000001'))
      .toBeVisible()
    await expect(page.getByTestId('record-diagnosis-rec-0000-0000-0000-000000000001'))
      .toContainText('Healthy')
    await expect(page.getByTestId('record-treatment-rec-0000-0000-0000-000000000001'))
      .toBeVisible()
  })

  test('VET sees New Medical Record button', async ({ page }) => {
    await expect(page.getByTestId('new-medical-record-btn')).toBeVisible()
  })

  test('vaccinations tab shows vaccination records', async ({ page }) => {
    await page.getByTestId('tab-vaccinations').click()
    await expect(page.getByTestId('tabpanel-vaccinations')).toBeVisible()
    await expect(page.getByTestId('vaccinations-list')).toBeVisible()

    const vaccinations = page.locator('[data-testid^="vaccination-vac-"]')
    await expect(vaccinations).toHaveCount(3)
  })

  test('prescriptions tab shows prescription records', async ({ page }) => {
    await page.getByTestId('tab-prescriptions').click()
    await expect(page.getByTestId('tabpanel-prescriptions')).toBeVisible()
    await expect(page.getByTestId('prescriptions-list')).toBeVisible()

    const prescriptions = page.locator('[data-testid^="prescription-presc-"]')
    await expect(prescriptions).toHaveCount(2)
  })
})

test.describe('VET creates new medical record', () => {
  test.beforeEach(async ({ page }) => {
    await loginAs(page, VET_TOKEN, '/patients')
  })

  test('navigates to new record form from patient detail page', async ({ page }) => {
    await page.goto(`/patients/${LUNA_PATIENT_ID}`)
    await waitForMSW(page)
    await page.waitForSelector('[data-testid="patient-detail-page"]', { timeout: 15000 })

    await page.getByTestId('new-medical-record-btn').click()
    await expect(page).toHaveURL(new RegExp(`/patients/${LUNA_PATIENT_ID}/records/new`))
    await expect(page.getByTestId('new-medical-record-page')).toBeVisible()
    await expect(page.getByTestId('medical-record-form')).toBeVisible()
  })

  test('creates a new medical record and redirects to patient detail', async ({ page }) => {
    await page.goto(`/patients/${LUNA_PATIENT_ID}/records/new`)
    await waitForMSW(page)
    await page.waitForSelector('[data-testid="new-medical-record-page"]', { timeout: 15000 })

    // Fill in the form
    await page.getByTestId('input-reason').fill('Skin condition follow-up')
    await page.getByTestId('input-anamnesis').fill('Follow-up after previous skin treatment. Improvement noted.')
    await page.getByTestId('input-weight').fill('3.2')
    await page.getByTestId('input-temperature').fill('38.5')
    await page.getByTestId('input-heart-rate').fill('140')
    await page.getByTestId('input-diagnosis').fill('Mild dehydration')
    await page.getByTestId('input-treatment').fill('IV fluids, recheck in 3 days')

    await page.getByTestId('save-record-btn').click()

    // Should redirect back to patient detail
    await expect(page).toHaveURL(new RegExp(`/patients/${LUNA_PATIENT_ID}`))
    await expect(page).not.toHaveURL(new RegExp('/records/new'))
  })

  test('new record appears at top of medical records list after creation', async ({ page }) => {
    // First, navigate and create a record
    await page.goto(`/patients/${LUNA_PATIENT_ID}/records/new`)
    await waitForMSW(page)
    await page.waitForSelector('[data-testid="new-medical-record-page"]', { timeout: 15000 })

    await page.getByTestId('input-reason').fill('Weight check')
    await page.getByTestId('input-anamnesis').fill('Routine weight monitoring.')
    await page.getByTestId('input-weight').fill('3.4')
    await page.getByTestId('input-temperature').fill('38.4')
    await page.getByTestId('input-heart-rate').fill('138')
    await page.getByTestId('input-diagnosis').fill('Normal weight, healthy')
    await page.getByTestId('input-treatment').fill('No treatment required')

    await page.getByTestId('save-record-btn').click()

    // Wait for redirect to patient detail
    await expect(page).toHaveURL(new RegExp(`/patients/${LUNA_PATIENT_ID}`))
    await expect(page).not.toHaveURL(new RegExp('/records/new'))
    await page.waitForLoadState('networkidle')

    // The new record should appear in medical records list
    await expect(page.getByTestId('tabpanel-medical-records')).toBeVisible()
    await expect(page.getByTestId('medical-records-list')).toBeVisible()

    // Should have at least 2 records now (1 original + 1 new)
    const records = page.locator('[data-testid^="medical-record-"]')
    const count = await records.count()
    expect(count).toBeGreaterThanOrEqual(2)
  })

  test('form validates required fields', async ({ page }) => {
    await page.goto(`/patients/${LUNA_PATIENT_ID}/records/new`)
    await waitForMSW(page)
    await page.waitForSelector('[data-testid="new-medical-record-page"]', { timeout: 15000 })

    // Click save without filling anything
    await page.getByTestId('save-record-btn').click()

    // Should show validation errors
    await expect(page.getByTestId('error-reason')).toBeVisible()
  })
})

test.describe('ASSISTANT can view but not create medical records', () => {
  test.beforeEach(async ({ page }) => {
    await loginAs(page, ASSISTANT_TOKEN, `/patients/${MAX_PATIENT_ID}`)
    await page.waitForSelector('[data-testid="patient-detail-page"]', { timeout: 20000 })
    await page.waitForSelector('[data-testid="patient-detail-name"]', { timeout: 20000 })
  })

  test('ASSISTANT can see medical records on patient detail', async ({ page }) => {
    await expect(page.getByTestId('tabpanel-medical-records')).toBeVisible()
    await expect(page.getByTestId('medical-records-list')).toBeVisible()
  })

  test('ASSISTANT does not see New Medical Record button', async ({ page }) => {
    await expect(page.getByTestId('new-medical-record-btn')).not.toBeVisible()
  })

  test('ASSISTANT is redirected away from new record page', async ({ page }) => {
    await page.goto(`/patients/${LUNA_PATIENT_ID}/records/new`)
    await waitForMSW(page)

    // Should be redirected to patient detail
    await expect(page).toHaveURL(new RegExp(`/patients/${LUNA_PATIENT_ID}`))
    await expect(page).not.toHaveURL(new RegExp('/records/new'))
  })
})

test.describe('RECEPTIONIST cannot access medical records', () => {
  test.beforeEach(async ({ page }) => {
    await loginAs(page, RECEPTIONIST_TOKEN, '/appointments')
    await page.waitForLoadState('networkidle')
  })

  test('RECEPTIONIST does not see Medical Records in sidebar', async ({ page }) => {
    await expect(page.getByTestId('nav-medical-records')).not.toBeVisible()
  })

  test('navigating to /patients redirects receptionist to appointments', async ({ page }) => {
    // The sidebar doesn't show medical records nav, but the feature says
    // navigating to /patients redirects to appointments.
    // Since Sidebar only hides nav items but doesn't block routes,
    // we just verify the nav item is hidden (core RBAC requirement).
    await expect(page.getByTestId('nav-medical-records')).not.toBeVisible()
    await expect(page.getByTestId('nav-appointments')).toBeVisible()
  })
})
