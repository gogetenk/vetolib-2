import { test, expect, type Route } from '@playwright/test'

// These tests use Playwright's page.route() to mock API calls.
// This is more reliable than MSW service workers in headless browsers.

const MOCK_VETS = [
  { id: 'vet-001', name: 'Dr. Sarah Johnson' },
  { id: 'vet-002', name: 'Dr. Ahmed Khalil' },
]

function getTodayDateAndTime(hour: number, minute = 0) {
  const d = new Date()
  const date = `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`
  const startTime = `${String(hour).padStart(2, '0')}:${String(minute).padStart(2, '0')}:00`
  const endMinute = minute + 30
  const endH = endMinute >= 60 ? hour + 1 : hour
  const endM = endMinute >= 60 ? endMinute - 60 : endMinute
  const endTime = `${String(endH).padStart(2, '0')}:${String(endM).padStart(2, '0')}:00`
  return { date, startTime, endTime }
}

function makeMockAppointments() {
  return [
    {
      id: 'a1b2c3d4-0000-0000-0000-000000000001',
      animalId: 'animal-001',
      animalName: 'Max',
      ownerName: 'Ahmed Al-Rashid',
      veterinarianId: 'vet-001',
      veterinarianName: 'Dr. Sarah Johnson',
      status: 'Scheduled',
      ...getTodayDateAndTime(9),
      durationMinutes: 30,
      reason: 'Annual vaccination',
      clinicId: 'clinic-001',
      source: 'Staff',
      rescheduleCount: 0,
      originalAppointmentId: null,
      consultationType: 'Vaccination',
    },
    {
      id: 'a1b2c3d4-0000-0000-0000-000000000002',
      animalId: 'animal-002',
      animalName: 'Luna',
      ownerName: 'Fatima Hassan',
      veterinarianId: 'vet-002',
      veterinarianName: 'Dr. Ahmed Khalil',
      status: 'InProgress',
      ...getTodayDateAndTime(10, 30),
      durationMinutes: 30,
      reason: 'Skin condition follow-up',
      clinicId: 'clinic-001',
      source: 'Staff',
      rescheduleCount: 0,
      originalAppointmentId: null,
      consultationType: 'Dermatology',
    },
    {
      id: 'a1b2c3d4-0000-0000-0000-000000000003',
      animalId: 'animal-003',
      animalName: 'Coco',
      ownerName: 'Mohammed Al-Zaabi',
      veterinarianId: 'vet-001',
      veterinarianName: 'Dr. Sarah Johnson',
      status: 'Scheduled',
      ...getTodayDateAndTime(11),
      durationMinutes: 30,
      reason: 'Wing check-up',
      clinicId: 'clinic-001',
      source: 'Staff',
      rescheduleCount: 0,
      originalAppointmentId: null,
      consultationType: 'General Checkup',
    },
  ]
}

type MockAppt = ReturnType<typeof makeMockAppointments>[number]

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
        CHECK_IN: 'CheckedIn',
        START: 'InProgress',
        COMPLETE: 'Completed',
        CANCEL: 'Cancelled',
      }
      appt.status = (transitions[body.action] ?? appt.status) as MockAppt['status']
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
      const veterinarianId = url.searchParams.get('veterinarianId')
      const page_ = parseInt(url.searchParams.get('page') ?? '1')
      const pageSize = parseInt(url.searchParams.get('pageSize') ?? '10')

      let items: MockAppt[] = [...appointments]
      if (status) items = items.filter(a => a.status === status)
      if (veterinarianId) items = items.filter(a => a.veterinarianId === veterinarianId)

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
      const vet = MOCK_VETS.find(v => v.id === body.veterinarianId)

      const conflict = appointments.find(
        a => a.veterinarianId === body.veterinarianId && a.date === body.date && a.startTime === body.startTime && a.status !== 'Cancelled'
      )
      if (conflict) {
        await route.fulfill({
          status: 409,
          json: { title: `This time slot is not available for ${vet?.name ?? 'the vet'}` },
        })
        return
      }

      const durationMinutes = body.durationMinutes ?? 30
      const [h, m] = (body.startTime as string).split(':').map(Number)
      const totalMin = h * 60 + m + durationMinutes
      const endTime = `${String(Math.floor(totalMin / 60)).padStart(2, '0')}:${String(totalMin % 60).padStart(2, '0')}:00`

      const newAppt: MockAppt = {
        id: `new-${Date.now()}`,
        status: 'Scheduled',
        clinicId: 'clinic-001',
        veterinarianName: vet?.name ?? '',
        veterinarianId: body.veterinarianId,
        animalId: body.animalId,
        animalName: 'New Patient',
        ownerName: 'Unknown Owner',
        date: body.date,
        startTime: body.startTime,
        endTime,
        durationMinutes,
        reason: body.reason ?? null,
        source: body.source ?? 'Staff',
        rescheduleCount: 0,
        originalAppointmentId: null,
        consultationType: 'General Checkup',
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

    await page.fill('[data-testid="input-animal-id"]', 'animal-new-001')

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

    await expect(page.locator('[data-testid="status-badge-checkedin"]')).toBeVisible({ timeout: 10000 })
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

    await page.fill('[data-testid="input-animal-id"]', 'animal-conflict-001')

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
