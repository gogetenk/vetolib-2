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

const ADMIN_TOKEN = makeToken('ADMIN', 'Dr. Khalid Al-Mansoori', 'admin@desertpaws.ae')
const VET_TOKEN = makeToken('VET', 'Dr. Sarah Johnson', 'dr.sarah@desertpaws.ae')
const ASSISTANT_TOKEN = makeToken('ASSISTANT', 'Mariam Al-Zaabi', 'assistant@desertpaws.ae')

// ─── Auth helpers ─────────────────────────────────────────────────────────────

async function waitForMSW(page: Page): Promise<void> {
  await page.waitForSelector('[data-testid="msw-ready"]', { state: 'attached', timeout: 15000 })
}

async function loginAs(page: Page, token: string): Promise<void> {
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
  await page.goto('/en/settings/preferences')
  await waitForMSW(page)
}

async function goToPreferences(page: Page, token: string): Promise<void> {
  await loginAs(page, token)
  await page.waitForSelector('[data-testid="preferences-page"]', { timeout: 20000 })
}

// ─────────────────────────────────────────────────────────────────────────────
// SECTION 1: Page structure
// ─────────────────────────────────────────────────────────────────────────────

test.describe('Preferences — page structure', () => {
  test('user sees all four preference categories', async ({ page }) => {
    await goToPreferences(page, VET_TOKEN)

    await expect(page.getByTestId('pref-section-Notifications')).toBeVisible()
    await expect(page.getByTestId('pref-section-AIFeatures')).toBeVisible()
    await expect(page.getByTestId('pref-section-Privacy')).toBeVisible()
    await expect(page.getByTestId('pref-section-Communication')).toBeVisible()
  })

  test('page title is visible', async ({ page }) => {
    await goToPreferences(page, VET_TOKEN)

    await expect(page.locator('h1')).toContainText('Preferences')
  })

  test('notification toggles are visible', async ({ page }) => {
    await goToPreferences(page, VET_TOKEN)

    await expect(page.getByTestId('pref-toggle-notifications.email')).toBeVisible()
    await expect(page.getByTestId('pref-toggle-notifications.appointment_reminders')).toBeVisible()
    await expect(page.getByTestId('pref-toggle-notifications.invoice')).toBeVisible()
  })
})

// ─────────────────────────────────────────────────────────────────────────────
// SECTION 2: Toggle behaviour
// ─────────────────────────────────────────────────────────────────────────────

test.describe('Preferences — toggle interactions', () => {
  test('user toggles email notifications OFF', async ({ page }) => {
    await goToPreferences(page, VET_TOKEN)

    const toggle = page.getByTestId('pref-toggle-notifications.email')
    // Email notifications default to ON (true)
    await expect(toggle).toHaveAttribute('aria-checked', 'true')

    await toggle.click()

    await expect(toggle).toHaveAttribute('aria-checked', 'false')
  })

  test('user toggles appointment reminders ON then OFF', async ({ page }) => {
    await goToPreferences(page, VET_TOKEN)

    const toggle = page.getByTestId('pref-toggle-notifications.appointment_reminders')
    await expect(toggle).toHaveAttribute('aria-checked', 'true')

    await toggle.click()
    await expect(toggle).toHaveAttribute('aria-checked', 'false')

    await toggle.click()
    await expect(toggle).toHaveAttribute('aria-checked', 'true')
  })

  test('source badge updates to "Your setting" after user toggles', async ({ page }) => {
    await goToPreferences(page, VET_TOKEN)

    const toggle = page.getByTestId('pref-toggle-notifications.invoice')
    await toggle.click()

    const badge = page.getByTestId('pref-source-badge-notifications.invoice')
    await expect(badge).toContainText('Your setting')
  })
})

// ─────────────────────────────────────────────────────────────────────────────
// SECTION 3: Drug interaction alert — disabled toggle
// ─────────────────────────────────────────────────────────────────────────────

test.describe('Preferences — drug interaction alert safety lock', () => {
  test('drug interaction toggle is disabled', async ({ page }) => {
    await goToPreferences(page, VET_TOKEN)

    const toggle = page.getByTestId('pref-toggle-ai.drug_interaction_alerts')
    await expect(toggle).toBeDisabled()
  })

  test('drug interaction toggle remains ON and cannot be turned off', async ({ page }) => {
    await goToPreferences(page, VET_TOKEN)

    const toggle = page.getByTestId('pref-toggle-ai.drug_interaction_alerts')
    await expect(toggle).toHaveAttribute('aria-checked', 'true')

    // Clicking a disabled element should not change the state
    await toggle.click({ force: true })
    await expect(toggle).toHaveAttribute('aria-checked', 'true')
  })

  test('drug interaction tooltip trigger is present', async ({ page }) => {
    await goToPreferences(page, VET_TOKEN)

    await expect(page.getByTestId('pref-drug-interaction-tooltip')).toBeVisible()
  })
})

// ─────────────────────────────────────────────────────────────────────────────
// SECTION 4: RBAC — AI features
// ─────────────────────────────────────────────────────────────────────────────

test.describe('Preferences — AI features RBAC', () => {
  test('non-admin (VET) sees AI toggles as disabled (read-only)', async ({ page }) => {
    await goToPreferences(page, VET_TOKEN)

    const triageToggle = page.getByTestId('pref-toggle-ai.triage_suggestions')
    await expect(triageToggle).toBeDisabled()

    const noShowToggle = page.getByTestId('pref-toggle-ai.no_show_predictions')
    await expect(noShowToggle).toBeDisabled()

    const messagingToggle = page.getByTestId('pref-toggle-ai.messaging_assistance')
    await expect(messagingToggle).toBeDisabled()
  })

  test('non-admin (ASSISTANT) sees "Contact your clinic admin" hint on AI toggles', async ({ page }) => {
    await goToPreferences(page, ASSISTANT_TOKEN)

    const section = page.getByTestId('pref-section-AIFeatures')
    // Should contain admin hint text
    await expect(section).toContainText('Contact your clinic admin')
  })

  test('admin can modify AI feature toggles', async ({ page }) => {
    await goToPreferences(page, ADMIN_TOKEN)

    const messagingToggle = page.getByTestId('pref-toggle-ai.messaging_assistance')
    // Default is OFF for messaging assistance
    await expect(messagingToggle).toHaveAttribute('aria-checked', 'false')
    await expect(messagingToggle).not.toBeDisabled()

    await messagingToggle.click()
    await expect(messagingToggle).toHaveAttribute('aria-checked', 'true')
  })

  test('admin sees triage suggestions toggle as enabled and modifiable', async ({ page }) => {
    await goToPreferences(page, ADMIN_TOKEN)

    const triageToggle = page.getByTestId('pref-toggle-ai.triage_suggestions')
    await expect(triageToggle).not.toBeDisabled()
    await expect(triageToggle).toHaveAttribute('aria-checked', 'true')
  })
})

// ─────────────────────────────────────────────────────────────────────────────
// SECTION 5: Privacy — consent revocation
// ─────────────────────────────────────────────────────────────────────────────

test.describe('Preferences — consent revocation', () => {
  test('admin sees consent management section', async ({ page }) => {
    await goToPreferences(page, ADMIN_TOKEN)

    await expect(page.getByTestId('consent-management-section')).toBeVisible()
    await expect(page.getByTestId('revoke-analytics-consent-btn')).toBeVisible()
  })

  test('non-admin does not see consent management section', async ({ page }) => {
    await goToPreferences(page, VET_TOKEN)

    await expect(page.getByTestId('consent-management-section')).not.toBeVisible()
  })

  test('admin can revoke analytics consent', async ({ page }) => {
    await goToPreferences(page, ADMIN_TOKEN)

    await page.getByTestId('revoke-analytics-consent-btn').click()

    // After revoking, analytics_tracking toggle should be OFF
    const analyticsToggle = page.getByTestId('pref-toggle-privacy.analytics_tracking')
    await expect(analyticsToggle).toHaveAttribute('aria-checked', 'false', { timeout: 5000 })
  })
})

// ─────────────────────────────────────────────────────────────────────────────
// SECTION 6: Source badges
// ─────────────────────────────────────────────────────────────────────────────

test.describe('Preferences — source badges', () => {
  test('system default items show "System default" badge', async ({ page }) => {
    await goToPreferences(page, VET_TOKEN)

    // Drug interaction alerts is always system_default
    const badge = page.getByTestId('pref-source-badge-ai.drug_interaction_alerts')
    await expect(badge).toContainText('System default')
  })

  test('source badge is visible for all preference items', async ({ page }) => {
    await goToPreferences(page, VET_TOKEN)

    const badges = page.locator('[data-testid^="pref-source-badge-"]')
    const count = await badges.count()
    expect(count).toBeGreaterThan(0)
  })
})

// ─────────────────────────────────────────────────────────────────────────────
// SECTION 7: Communication section
// ─────────────────────────────────────────────────────────────────────────────

test.describe('Preferences — communication settings', () => {
  test('communication section contains quiet hours inputs', async ({ page }) => {
    await goToPreferences(page, VET_TOKEN)

    await expect(page.getByTestId('pref-time-communication.quiet_hours_start')).toBeVisible()
    await expect(page.getByTestId('pref-time-communication.quiet_hours_end')).toBeVisible()
  })

  test('communication section contains preferred channel select', async ({ page }) => {
    await goToPreferences(page, VET_TOKEN)

    await expect(page.getByTestId('pref-select-communication.preferred_channel')).toBeVisible()
  })

  test('communication section contains language select', async ({ page }) => {
    await goToPreferences(page, VET_TOKEN)

    await expect(page.getByTestId('pref-select-communication.language')).toBeVisible()
  })
})

// ─────────────────────────────────────────────────────────────────────────────
// SECTION 8: Persistence
// ─────────────────────────────────────────────────────────────────────────────

test.describe('Preferences — persistence', () => {
  test('toggled preference change is reflected immediately on same page load', async ({ page }) => {
    await goToPreferences(page, VET_TOKEN)

    // Email starts ON, invoice starts ON — toggle both
    const emailToggle = page.getByTestId('pref-toggle-notifications.email')
    const invoiceToggle = page.getByTestId('pref-toggle-notifications.invoice')

    await expect(emailToggle).toHaveAttribute('aria-checked', 'true')
    await expect(invoiceToggle).toHaveAttribute('aria-checked', 'true')

    await emailToggle.click()
    await invoiceToggle.click()

    // Both should be OFF immediately (optimistic update)
    await expect(emailToggle).toHaveAttribute('aria-checked', 'false')
    await expect(invoiceToggle).toHaveAttribute('aria-checked', 'false')

    // Toggle back ON
    await emailToggle.click()
    await expect(emailToggle).toHaveAttribute('aria-checked', 'true')

    // Invoice still OFF
    await expect(invoiceToggle).toHaveAttribute('aria-checked', 'false')
  })
})
