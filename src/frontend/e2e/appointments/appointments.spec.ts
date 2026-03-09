import { test, expect, type Route } from '@playwright/test'

// These tests use Playwright's page.route() to mock API calls.
// This is more reliable than MSW service workers in headless browsers.

const MOCK_VETS = [
  { id: 'vet-001', name: 'Dr. Sarah Johnson' },
  { id: 'vet-002', name: 'Dr. Ahmed Khalil' },
]

function getTodayAt(hour: number, minute = 0) {
  const d = new Date()
  d.setHours(hour, minute, 0, 0)
  return d.toISOString()
}

function makeMockAppointments() {
  return [
    {
      id: 'a1b2c3d4-0000-0000-0000-000000000001',
      patientName: 'Max',
      species: 'Dog',
      ownerName: 'Ahmed Al-Rashid',
      ownerPhone: '+971 50 123 4567',
      vetId: 'vet-001',
      vetName: 'Dr. Sarah Johnson',
      status: 'SCHEDULED',
      scheduledAt: getTodayAt(9),
      reason: 'Annual vaccination',
      clinicId: 'clinic-001',
    },
    {
      id: 'a1b2c3d4-0000-0000-0000-000000000002',
      patientName: 'Luna',
      species: 'Cat',
      ownerName: 'Fatima Hassan',
      ownerPhone: '+971 55 987 6543',
      vetId: 'vet-002',
      vetName: 'Dr. Ahmed Khalil',
      status: 'IN_PROGRESS',
      scheduledAt: getTodayAt(10, 30),
      reason: 'Skin condition follow-up',
      clinicId: 'clinic-001',
    },
    {
      id: 'a1b2c3d4-0000-0000-0000-000000000003',
      patientName: 'Coco',
      species: 'Bird',
      ownerName: 'Mohammed Al-Zaabi',
      ownerPhone: '+971 52 345 6789',
      vetId: 'vet-001',
      vetName: 'Dr. Sarah Johnson',
      status: 'SCHEDULED',
      scheduledAt: getTodayAt(11),
      reason: 'Wing check-up',
      clinicId: 'clinic-001',
    },
  ]
}

type MockAppt = ReturnType<typeof makeMockAppointments>[number] & {
  cancellationReason?: string
}

async function setupRoutes(page: import('@playwright/test').Page) {
  const appointments: MockAppt[] = makeMockAppointments()

  await page.route('**/api/**', async (route: Route) => {
    const request = route.request()
    const url = new URL(request.url())
    const path = url.pathname
    const method = request.method()

    // GET /api/vets
    if (method === 'GET' && path === '/api/vets') {
      await route.fulfill({ json: MOCK_VETS })
      return
    }

    // PATCH /api/appointments/:id/transition
    if (method === 'PATCH' && path.match(/\/api\/appointments\/[^/]+\/transition/)) {
      const id = path.split('/api/appointments/')[1].split('/transition')[0]
      const body = JSON.parse(request.postData() ?? '{}')
      const appt = appointments.find(a => a.id === id)
      if (!appt) {
        await route.fulfill({ status: 404 })
        return
      }
      const transitions: Record<string, string> = {
        CHECK_IN: 'CHECKED_IN',
        START: 'IN_PROGRESS',
        COMPLETE: 'COMPLETED',
        CANCEL: 'CANCELLED',
      }
      appt.status = (transitions[body.action] ?? appt.status) as MockAppt['status']
      if (body.action === 'CANCEL' && body.reason) {
        appt.cancellationReason = body.reason
      }
      await route.fulfill({ json: appt })
      return
    }

    // GET /api/appointments/:id
    if (method === 'GET' && path.match(/\/api\/appointments\/[^/]+$/)) {
      const id = path.split('/api/appointments/')[1]
      const appt = appointments.find(a => a.id === id)
      if (!appt) {
        await route.fulfill({ status: 404 })
      } else {
        await route.fulfill({ json: appt })
      }
      return
    }

    // GET /api/appointments (list)
    if (method === 'GET' && path === '/api/appointments') {
      const status = url.searchParams.get('status')
      const vetId = url.searchParams.get('vetId')
      const page_ = parseInt(url.searchParams.get('page') ?? '1')
      const pageSize = parseInt(url.searchParams.get('pageSize') ?? '10')

      let items: MockAppt[] = [...appointments]
      if (status) items = items.filter(a => a.status === status)
      if (vetId) items = items.filter(a => a.vetId === vetId)

      const totalCount = items.length
      const paged = items.slice((page_ - 1) * pageSize, page_ * pageSize)

      await route.fulfill({
        json: { items: paged, totalCount, page: page_, pageSize },
      })
      return
    }

    // POST /api/appointments
    if (method === 'POST' && path === '/api/appointments') {
      const body = JSON.parse(request.postData() ?? '{}')
      const vet = MOCK_VETS.find(v => v.id === body.vetId)

      const conflict = appointments.find(
        a => a.vetId === body.vetId && a.scheduledAt === body.scheduledAt && a.status !== 'CANCELLED'
      )
      if (conflict) {
        await route.fulfill({
          status: 409,
          json: { title: `This time slot is not available for ${vet?.name ?? 'the vet'}` },
        })
        return
      }

      const newAppt: MockAppt = {
        id: `new-${Date.now()}`,
        status: 'SCHEDULED',
        clinicId: 'clinic-001',
        vetName: vet?.name ?? '',
        patientName: body.patientName,
        species: body.species,
        ownerName: body.ownerName,
        ownerPhone: body.ownerPhone,
        vetId: body.vetId,
        scheduledAt: body.scheduledAt,
        reason: body.reason,
      }
      appointments.push(newAppt)
      await route.fulfill({ status: 201, json: newAppt })
      return
    }

    // Fallback — allow unmatched requests through
    await route.continue()
  })
}

async function authenticate(context: import('@playwright/test').BrowserContext) {
  await context.addCookies([
    {
      name: 'access_token',
      value: 'mock-access-token',
      domain: 'localhost',
      path: '/',
      httpOnly: false,
      secure: false,
    },
  ])
}

test.describe('Appointments', () => {
  test.beforeEach(async ({ page, context }) => {
    await authenticate(context)
    // Disable MSW service worker so page.route() can intercept API calls directly.
    // MSW service workers run at a lower level than Playwright's CDP route interceptor,
    // causing "bypass" requests to skip page.route() entirely.
    // Setting window.__DISABLE_MSW__ = true is checked by MSWProvider before starting the worker.
    await page.addInitScript(() => {
      ;(window as unknown as Record<string, unknown>).__DISABLE_MSW__ = true
    })
    await setupRoutes(page)
    await page.goto('/appointments')
    // Wait for table to render and loading to finish
    await page.waitForSelector('[data-testid="appointments-table"]', { timeout: 15000 })
    await page.waitForFunction(
      () => !document.querySelector('[data-testid="loading-indicator"]'),
      { timeout: 15000 }
    )
  })

  test('Receptionist sees today appointments list', async ({ page }) => {
    const rows = page.locator('[data-testid^="appointment-row-"]')
    await expect(rows).toHaveCount(3, { timeout: 10000 })

    const firstRow = rows.first()
    await expect(firstRow.locator('[data-testid="cell-patient"]')).toBeVisible()
    await expect(firstRow.locator('[data-testid="cell-vet"]')).toBeVisible()
    await expect(firstRow.locator('[data-testid^="status-badge-"]')).toBeVisible()
  })

  test('New Appointment button is visible', async ({ page }) => {
    await expect(page.locator('[data-testid="new-appointment-btn"]')).toBeVisible()
  })

  test('Create a new appointment', async ({ page }) => {
    await page.click('[data-testid="new-appointment-btn"]')
    await page.waitForSelector('[data-testid="appointment-form"]', { timeout: 10000 })

    await page.fill('[data-testid="input-patient-name"]', 'Simba')

    await page.click('[data-testid="select-species-trigger"]')
    await page.waitForSelector('[data-testid="species-option-cat"]', { timeout: 5000 })
    await page.click('[data-testid="species-option-cat"]')

    await page.fill('[data-testid="input-owner-name"]', 'Khalid Al-Mansoori')
    await page.fill('[data-testid="input-owner-phone"]', '+971 50 999 0001')

    await page.click('[data-testid="select-vet-trigger"]')
    await page.waitForSelector('[data-testid="vet-option-vet-001"]', { timeout: 10000 })
    await page.click('[data-testid="vet-option-vet-001"]')

    const tomorrow = new Date()
    tomorrow.setDate(tomorrow.getDate() + 1)
    const dateStr = tomorrow.toISOString().split('T')[0]
    await page.fill('[data-testid="input-date"]', dateStr)

    await page.click('[data-testid="select-time-trigger"]')
    await page.waitForSelector('[data-testid="time-option-1030"]', { timeout: 5000 })
    await page.click('[data-testid="time-option-1030"]')

    await page.fill('[data-testid="textarea-reason"]', 'Annual check-up')
    await page.click('[data-testid="btn-save"]')

    await page.waitForURL('**/appointments', { timeout: 10000 })
    await expect(page.locator('[data-testid="appointments-table"]')).toBeVisible()
  })

  test('Check in a patient on arrival', async ({ page }) => {
    await expect(page.locator('[data-testid^="appointment-row-"]')).toHaveCount(3, { timeout: 10000 })

    const scheduledRow = page.locator('[data-testid^="appointment-row-"]').filter({
      has: page.locator('[data-testid="status-badge-scheduled"]'),
    }).first()

    await scheduledRow.locator('[data-testid^="btn-view-"]').click()
    await page.waitForSelector('[data-testid="appointment-detail"]', { timeout: 10000 })

    await page.click('[data-testid="btn-action-check-in"]')
    await page.waitForSelector('[data-testid="confirm-dialog"]')
    await page.click('[data-testid="btn-dialog-confirm"]')

    await expect(page.locator('[data-testid="status-badge-checked_in"]')).toBeVisible({ timeout: 10000 })
  })

  test('Cancel an appointment with reason', async ({ page }) => {
    await expect(page.locator('[data-testid^="appointment-row-"]')).toHaveCount(3, { timeout: 10000 })

    const scheduledRow = page.locator('[data-testid^="appointment-row-"]').filter({
      has: page.locator('[data-testid="status-badge-scheduled"]'),
    }).first()

    await scheduledRow.locator('[data-testid^="btn-view-"]').click()
    await page.waitForSelector('[data-testid="appointment-detail"]', { timeout: 10000 })

    await page.click('[data-testid="btn-action-cancel"]')
    await page.waitForSelector('[data-testid="confirm-dialog"]')
    await page.fill('[data-testid="input-cancel-reason"]', 'Owner called to cancel')
    await page.click('[data-testid="btn-dialog-confirm"]')

    await expect(page.locator('[data-testid="status-badge-cancelled"]')).toBeVisible({ timeout: 10000 })
    await expect(page.locator('[data-testid="detail-cancellation-reason"]')).toContainText(
      'Owner called to cancel'
    )
  })

  test('Filter appointments by status', async ({ page }) => {
    await expect(page.locator('[data-testid^="appointment-row-"]')).toHaveCount(3, { timeout: 10000 })

    await page.click('[data-testid="status-filter"]')
    await page.waitForSelector('[data-testid="status-option-scheduled"]', { timeout: 5000 })
    await page.click('[data-testid="status-option-scheduled"]')

    await page.waitForFunction(
      () => !document.querySelector('[data-testid="loading-indicator"]'),
      { timeout: 10000 }
    )

    const badges = page.locator('[data-testid^="status-badge-"]')
    const allBadges = await badges.all()
    expect(allBadges.length).toBeGreaterThan(0)
    for (const badge of allBadges) {
      await expect(badge).toHaveAttribute('data-testid', 'status-badge-scheduled')
    }
  })

  test('Slot conflict shows error message', async ({ page }) => {
    await page.click('[data-testid="new-appointment-btn"]')
    await page.waitForSelector('[data-testid="appointment-form"]', { timeout: 10000 })

    await page.fill('[data-testid="input-patient-name"]', 'Rocky')
    await page.click('[data-testid="select-species-trigger"]')
    await page.waitForSelector('[data-testid="species-option-dog"]', { timeout: 5000 })
    await page.click('[data-testid="species-option-dog"]')
    await page.fill('[data-testid="input-owner-name"]', 'Test Owner')
    await page.fill('[data-testid="input-owner-phone"]', '+971 55 000 0000')

    await page.click('[data-testid="select-vet-trigger"]')
    await page.waitForSelector('[data-testid="vet-option-vet-001"]', { timeout: 10000 })
    await page.click('[data-testid="vet-option-vet-001"]')

    // Use today's date with the same time as Max's appointment (09:00)
    const today = new Date().toISOString().split('T')[0]
    await page.fill('[data-testid="input-date"]', today)
    await page.click('[data-testid="select-time-trigger"]')
    await page.waitForSelector('[data-testid="time-option-0900"]', { timeout: 5000 })
    await page.click('[data-testid="time-option-0900"]')

    await page.fill('[data-testid="textarea-reason"]', 'Test conflict')
    await page.click('[data-testid="btn-save"]')

    await expect(page.locator('text=not available')).toBeVisible({ timeout: 10000 })
    await expect(page.locator('[data-testid="appointment-form"]')).toBeVisible()
  })
})
