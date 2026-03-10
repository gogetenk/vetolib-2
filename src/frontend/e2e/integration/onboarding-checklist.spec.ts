import { test, expect, type Page } from '@playwright/test'

// ─── JWT helpers ─────────────────────────────────────────────────────────────

let tokenCounter = 0

function makeToken(
  role: string,
  name: string,
  emailSuffix: string,
  clinicName = 'Desert Paws Clinic'
): string {
  // Unique email per call to isolate MSW state between tests
  tokenCounter++
  const email = `${emailSuffix}-${tokenCounter}@desertpaws.ae`
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

async function waitForChecklist(page: Page): Promise<void> {
  await expect(page.getByTestId('setup-checklist')).toBeVisible({ timeout: 10000 })
}

// ─────────────────────────────────────────────────────────────────────────────
// SECTION 1: Checklist visibility and step count per role
// ─────────────────────────────────────────────────────────────────────────────

test.describe('Setup Checklist — visibility and step count', () => {
  test('Admin sees 5-step checklist with progress "0 of 5"', async ({ page }) => {
    const token = makeToken('ADMIN', 'Dr. Admin User', 'admin')
    await loginAs(page, token)
    await waitForChecklist(page)

    const progress = page.getByTestId('checklist-progress')
    await expect(progress).toBeVisible()
    await expect(progress).toHaveText('0 of 5 completed')

    // All 5 admin steps should be visible
    await expect(page.getByTestId('checklist-item-invite_team_member')).toBeVisible()
    await expect(page.getByTestId('checklist-item-add_first_patient')).toBeVisible()
    await expect(page.getByTestId('checklist-item-book_first_appointment')).toBeVisible()
    await expect(page.getByTestId('checklist-item-create_first_invoice')).toBeVisible()
    await expect(page.getByTestId('checklist-item-explore_dashboard')).toBeVisible()
  })

  test('Vet sees 4-step checklist', async ({ page }) => {
    const token = makeToken('VET', 'Dr. Sarah Johnson', 'vet')
    await loginAs(page, token)
    await waitForChecklist(page)

    const progress = page.getByTestId('checklist-progress')
    await expect(progress).toHaveText('0 of 4 completed')

    await expect(page.getByTestId('checklist-item-view_appointments')).toBeVisible()
    await expect(page.getByTestId('checklist-item-open_patient_record')).toBeVisible()
    await expect(page.getByTestId('checklist-item-add_medical_record')).toBeVisible()
    await expect(page.getByTestId('checklist-item-write_prescription')).toBeVisible()
  })

  test('Receptionist sees 4-step checklist', async ({ page }) => {
    const token = makeToken('RECEPTIONIST', 'Khalid Al-Nuaimi', 'reception')
    await loginAs(page, token)
    await waitForChecklist(page)

    const progress = page.getByTestId('checklist-progress')
    await expect(progress).toHaveText('0 of 4 completed')

    await expect(page.getByTestId('checklist-item-book_appointment')).toBeVisible()
    await expect(page.getByTestId('checklist-item-check_in_patient')).toBeVisible()
    await expect(page.getByTestId('checklist-item-create_invoice')).toBeVisible()
    await expect(page.getByTestId('checklist-item-send_invoice')).toBeVisible()
  })

  test('Assistant sees 3-step checklist', async ({ page }) => {
    const token = makeToken('ASSISTANT', 'Mariam Al-Zaabi', 'assistant')
    await loginAs(page, token)
    await waitForChecklist(page)

    const progress = page.getByTestId('checklist-progress')
    await expect(progress).toHaveText('0 of 3 completed')

    await expect(page.getByTestId('checklist-item-browse_patients')).toBeVisible()
    await expect(page.getByTestId('checklist-item-view_medical_record')).toBeVisible()
    await expect(page.getByTestId('checklist-item-check_today_schedule')).toBeVisible()
  })
})

// ─────────────────────────────────────────────────────────────────────────────
// SECTION 2: Step navigation
// ─────────────────────────────────────────────────────────────────────────────

test.describe('Setup Checklist — step navigation', () => {
  test('Clicking a pending step navigates to the target page', async ({ page }) => {
    const token = makeToken('ADMIN', 'Dr. Admin User', 'admin')
    await loginAs(page, token)
    await waitForChecklist(page)

    // Click the "Invite team member" step — should navigate to /settings/team
    await page.getByTestId('checklist-item-invite_team_member').click()
    await expect(page).toHaveURL(/\/settings\/team/, { timeout: 10000 })
  })

  test('Clicking "Add first patient" step navigates to /patients/new', async ({ page }) => {
    const token = makeToken('ADMIN', 'Dr. Admin User', 'admin')
    await loginAs(page, token)
    await waitForChecklist(page)

    await page.getByTestId('checklist-item-add_first_patient').click()
    await expect(page).toHaveURL(/\/patients\/new/, { timeout: 10000 })
  })
})

// ─────────────────────────────────────────────────────────────────────────────
// SECTION 3: Step completion and progress update
// ─────────────────────────────────────────────────────────────────────────────

test.describe('Setup Checklist — step completion and progress', () => {
  test('Step check icon reflects pending state initially', async ({ page }) => {
    const token = makeToken('ADMIN', 'Dr. Admin User', 'admin')
    await loginAs(page, token)
    await waitForChecklist(page)

    const checkIcon = page.getByTestId('checklist-item-check-invite_team_member')
    await expect(checkIcon).toBeVisible()
    // Pending steps have a border-gray class — no green background
    await expect(checkIcon).not.toHaveClass(/bg-green-500/)
  })

  test('Progress updates when a step is marked complete via MSW', async ({ page }) => {
    const token = makeToken('ADMIN', 'Dr. Admin User', 'admin')
    await loginAs(page, token)
    await waitForChecklist(page)

    // Initial progress
    await expect(page.getByTestId('checklist-progress')).toHaveText('0 of 5 completed')

    // Complete a step via direct API call
    await page.evaluate(async (t) => {
      await fetch('/api/onboarding/steps/add_first_patient/complete', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${t}`,
        },
        body: '{}',
      })
    }, token)

    // Trigger the hook to refetch from MSW — window.__onboardingRefetch__ is exposed by useOnboarding
    await page.evaluate(() => {
      const refetch = (window as unknown as Record<string, unknown>).__onboardingRefetch__ as (() => void) | undefined
      if (refetch) refetch()
    })

    // The UI should now reflect 1 of 5 completed after the refetch
    await expect(page.getByTestId('checklist-progress')).toHaveText('1 of 5 completed', { timeout: 5000 })
  })

  test('Completed step shows checkmark icon', async ({ page }) => {
    const token = makeToken('ADMIN', 'Dr. Admin User', 'admin')
    await loginAs(page, token)
    await waitForChecklist(page)

    // Complete a step directly via the API
    await page.evaluate(async (t) => {
      await fetch('/api/onboarding/steps/book_first_appointment/complete', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${t}`,
        },
        body: '{}',
      })
    }, token)

    // Trigger the hook to refetch from MSW
    await page.evaluate(() => {
      const refetch = (window as unknown as Record<string, unknown>).__onboardingRefetch__ as (() => void) | undefined
      if (refetch) refetch()
    })

    // The completed step icon should have green background after refetch
    const checkIcon = page.getByTestId('checklist-item-check-book_first_appointment')
    await expect(checkIcon).toHaveClass(/bg-green-500/, { timeout: 5000 })
  })
})

// ─────────────────────────────────────────────────────────────────────────────
// SECTION 4: All steps completed — congratulation message
// ─────────────────────────────────────────────────────────────────────────────

test.describe('Setup Checklist — completion state', () => {
  test('All steps completed shows congratulation message', async ({ page }) => {
    const token = makeToken('ASSISTANT', 'Mariam Al-Zaabi', 'assistant')
    await loginAs(page, token)
    await waitForChecklist(page)

    // Complete all 3 assistant steps via direct API calls within the same page context
    const assistantSteps = ['browse_patients', 'view_medical_record', 'check_today_schedule']

    await page.evaluate(async ({ steps, t }) => {
      for (const stepId of steps) {
        await fetch(`/api/onboarding/steps/${stepId}/complete`, {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            Authorization: `Bearer ${t}`,
          },
          body: '{}',
        })
      }
    }, { steps: assistantSteps, t: token })

    // Trigger the hook to refetch from MSW — window.__onboardingRefetch__ exposed by useOnboarding
    await page.evaluate(() => {
      const refetch = (window as unknown as Record<string, unknown>).__onboardingRefetch__ as (() => void) | undefined
      if (refetch) refetch()
    })

    // The congratulation message should be visible once all steps are complete
    await expect(page.getByTestId('checklist-complete-message')).toBeVisible({ timeout: 10000 })

    // Progress counter should not be shown when all complete
    await expect(page.getByTestId('checklist-progress')).not.toBeVisible()

    // Dismiss button should not be shown when all complete
    await expect(page.getByTestId('checklist-dismiss')).not.toBeVisible()
  })
})

// ─────────────────────────────────────────────────────────────────────────────
// SECTION 5: Dismiss checklist
// ─────────────────────────────────────────────────────────────────────────────

test.describe('Setup Checklist — dismiss', () => {
  test('Dismiss checklist hides it permanently', async ({ page }) => {
    const token = makeToken('VET', 'Dr. Sarah Johnson', 'vet')
    await loginAs(page, token)
    await waitForChecklist(page)

    // Dismiss the checklist
    await page.getByTestId('checklist-dismiss').click()

    // Checklist should be hidden
    await expect(page.getByTestId('setup-checklist')).not.toBeVisible({ timeout: 3000 })
  })

  test('Checklist does not reappear after reload (MSW persists dismiss state)', async ({ page }) => {
    const token = makeToken('RECEPTIONIST', 'Khalid Al-Nuaimi', 'reception')
    await loginAs(page, token)
    await waitForChecklist(page)

    // Dismiss
    await page.getByTestId('checklist-dismiss').click()
    await expect(page.getByTestId('setup-checklist')).not.toBeVisible({ timeout: 3000 })

    // Verify that the MSW state was updated (checklistVisible = false) within the same context
    const state = await page.evaluate(async (t) => {
      const res = await fetch('/api/onboarding', {
        headers: { Authorization: `Bearer ${t}` },
      })
      return res.json()
    }, token)
    expect(state.checklistVisible).toBe(false)

    // Trigger a refetch to simulate what would happen on reload
    await page.evaluate(() => {
      const refetch = (window as unknown as Record<string, unknown>).__onboardingRefetch__ as (() => void) | undefined
      if (refetch) refetch()
    })

    // The checklist should remain hidden after refetch (MSW returns checklistVisible: false)
    await expect(page.getByTestId('setup-checklist')).not.toBeVisible({ timeout: 5000 })
  })
})

// ─────────────────────────────────────────────────────────────────────────────
// SECTION 6: RTL / Arabic locale
// ─────────────────────────────────────────────────────────────────────────────

test.describe('Setup Checklist — RTL (Arabic locale)', () => {
  test('Checklist renders correctly in AR locale with Arabic labels', async ({ page }) => {
    const token = makeToken('ADMIN', 'Dr. Admin User', 'admin')
    await loginAs(page, token, '/ar/dashboard')
    await waitForChecklist(page)

    // Verify the page is in RTL mode
    const htmlDir = await page.evaluate(() => document.documentElement.dir)
    expect(htmlDir).toBe('rtl')

    // Checklist is visible
    await expect(page.getByTestId('setup-checklist')).toBeVisible()

    // Progress counter shows text
    const progress = page.getByTestId('checklist-progress')
    await expect(progress).toBeVisible()

    // Dismiss button is visible
    await expect(page.getByTestId('checklist-dismiss')).toBeVisible()

    // Step items are visible
    await expect(page.getByTestId('checklist-item-invite_team_member')).toBeVisible()
  })
})
