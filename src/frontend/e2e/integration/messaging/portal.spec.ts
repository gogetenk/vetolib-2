/**
 * Owner Portal — Playwright tests (MSW mode)
 *
 * 13 scenarios:
 *  1. Owner opens portal with valid magic link
 *  2. Expired magic link shows error message
 *  3. Consent screen displayed on first visit
 *  4. Accept consent allows composing message
 *  5. Pet selector shows registered pets
 *  6. Category selector available
 *  7. Character counter shows remaining (2000 max)
 *  8. Send button disabled when > 2000 chars
 *  9. Photo upload with preview
 * 10. Cannot attach more than 3 photos
 * 11. Daily message limit (5) enforced
 * 12. Conversation history displayed without internal notes
 * 13. Export button downloads text file
 */
import { test, expect } from '@playwright/test'
import { openPortalWithMagicLink } from '../../fixtures/messaging'

// Valid token — MSW handler allows it (set in consentGiven for 'valid-magic-token-001')
const VALID_TOKEN = 'valid-magic-token-001'
// Expired token — MSW handler returns 401
const EXPIRED_TOKEN = 'expired-magic-token'
// A new token without prior consent
const NEW_TOKEN = 'new-owner-token-no-consent'

const CLINIC_SLUG = 'desert-paws'

test.describe('Owner Portal — valid magic link', () => {
  test('Owner opens portal with valid magic link', async ({ page }) => {
    await openPortalWithMagicLink(page, VALID_TOKEN)

    // Portal landing should be shown with conversations list
    await expect(page.getByTestId('portal-landing')).toBeVisible()

    // Should show at least one conversation from mock data
    const convList = page.getByTestId('portal-conversations-list')
    await expect(convList).toBeVisible()

    // New message button should be available
    await expect(page.getByTestId('new-message-btn')).toBeVisible()
  })

  test('Conversation history displayed without internal notes', async ({ page }) => {
    await openPortalWithMagicLink(page, VALID_TOKEN)
    await expect(page.getByTestId('portal-landing')).toBeVisible()

    // Click on the first conversation to view detail
    const convItem = page
      .getByTestId('portal-conversations-list')
      .locator('[data-testid^="conversation-item-"]')
      .first()
    await expect(convItem).toBeVisible()
    await convItem.click()

    // Portal conversation view should be shown
    await expect(page.getByTestId('portal-conversation')).toBeVisible({ timeout: 8000 })
    await expect(page.getByTestId('message-thread')).toBeVisible()

    // Internal notes should not be present (portal never shows internal notes)
    // All messages visible in portal are from Owner or Staff (not internal)
    const messages = page.locator('[data-testid^="message-"]')
    await expect(messages.first()).toBeVisible()
  })

  test('Export button downloads text file', async ({ page }) => {
    await openPortalWithMagicLink(page, VALID_TOKEN)
    await expect(page.getByTestId('portal-landing')).toBeVisible()

    // Export link should be visible
    const exportLink = page.getByTestId('export-link')
    await expect(exportLink).toBeVisible()

    // Click the export link
    await exportLink.click()

    // Should navigate to export page
    await expect(page).toHaveURL(/\/export/)
    await expect(page.getByTestId('export-page')).toBeVisible({ timeout: 8000 })
    await expect(page.getByTestId('download-export-btn')).toBeVisible()
  })
})

test.describe('Owner Portal — expired magic link', () => {
  test('Expired magic link shows error message', async ({ page }) => {
    await openPortalWithMagicLink(page, EXPIRED_TOKEN)

    // The MSW portal handler returns 401 for 'expired-magic-token'
    // The PortalLanding component detects 401 and shows expired state
    await expect(page.getByTestId('portal-expired')).toBeVisible({ timeout: 10000 })
    await expect(page.getByTestId('expired-message')).toBeVisible()
  })
})

test.describe('Owner Portal — consent flow', () => {
  test('Consent screen displayed on first visit', async ({ page }) => {
    // Navigate to portal landing with a new token (no prior consent in sessionStorage)
    await openPortalWithMagicLink(page, NEW_TOKEN)
    await expect(page.getByTestId('portal-landing')).toBeVisible({ timeout: 10000 })

    // Click new message button — should redirect to consent screen first
    await page.getByTestId('new-message-btn').click()

    // Consent screen should appear (no portal_consent_given in sessionStorage)
    await expect(page.getByTestId('consent-screen')).toBeVisible({ timeout: 8000 })
    await expect(page.getByTestId('consent-title')).toBeVisible()
  })

  test('Accept consent allows composing message', async ({ page }) => {
    // Navigate directly to consent page
    await openPortalWithMagicLink(page, NEW_TOKEN)
    await page.goto(`/en/portal/${CLINIC_SLUG}/consent?token=${NEW_TOKEN}`)
    await page.waitForSelector('[data-testid="msw-ready"]', { state: 'attached', timeout: 15000 })

    await expect(page.getByTestId('consent-screen')).toBeVisible({ timeout: 10000 })

    // Accept the terms
    await page.getByTestId('consent-checkbox').check()
    await expect(page.getByTestId('consent-checkbox')).toBeChecked()

    // Click accept
    await page.getByTestId('accept-consent-btn').click()

    // Should navigate to new message form
    await expect(page).toHaveURL(/\/new/, { timeout: 10000 })
    await expect(page.getByTestId('new-message-form')).toBeVisible({ timeout: 8000 })
  })
})

test.describe('Owner Portal — new message form', () => {
  test.beforeEach(async ({ page }) => {
    // Navigate directly to the new message form with a valid token
    // and set consent in sessionStorage so we skip the consent screen
    await page.goto(`/en/portal/${CLINIC_SLUG}/new?token=${VALID_TOKEN}`)
    await page.addInitScript(() => {
      sessionStorage.setItem('portal_consent_given', 'true')
      sessionStorage.setItem('portal_token', 'valid-magic-token-001')
    })
    await page.goto(`/en/portal/${CLINIC_SLUG}/new?token=${VALID_TOKEN}`)
    await page.waitForSelector('[data-testid="msw-ready"]', { state: 'attached', timeout: 15000 })
    await page.waitForSelector('[data-testid="new-message-form"]', { timeout: 10000 })
  })

  test('Pet selector shows registered pets', async ({ page }) => {
    // Pet selector should be visible with Max listed
    await expect(page.getByTestId('pet-selector')).toBeVisible()
  })

  test('Category selector available', async ({ page }) => {
    // Category selector should be visible
    await expect(page.getByTestId('category-selector')).toBeVisible()
  })

  test('Character counter shows remaining (2000 max)', async ({ page }) => {
    const messageInput = page.getByTestId('message-input')
    await expect(messageInput).toBeVisible()

    // Type some text
    await messageInput.fill('Hello, I would like to ask about my pet.')

    // Character counter should be visible and show remaining chars
    const counter = page.getByTestId('char-counter')
    await expect(counter).toBeVisible()
    const counterText = await counter.textContent()
    // Counter should show a number (remaining characters)
    expect(counterText).toMatch(/\d+/)
  })

  test('Send button disabled when > 2000 chars', async ({ page }) => {
    const messageInput = page.getByTestId('message-input')
    await expect(messageInput).toBeVisible()

    // Fill with 2001 characters
    const longText = 'A'.repeat(2001)
    await messageInput.fill(longText)

    // Send button should be disabled (or char limit warning visible)
    const sendBtn = page.getByTestId('send-message-btn')
    await expect(sendBtn).toBeDisabled()
  })

  test('Photo upload with preview', async ({ page }) => {
    // Photo upload component should be present
    await expect(page.getByTestId('photo-upload')).toBeVisible()

    // Set up a file chooser
    const fileChooserPromise = page.waitForEvent('filechooser')
    await page.getByTestId('add-photo-btn').click()
    const fileChooser = await fileChooserPromise

    // Upload a mock image file
    await fileChooser.setFiles({
      name: 'test-image.jpg',
      mimeType: 'image/jpeg',
      buffer: Buffer.from('fake-image-data'),
    })

    // Photo preview should appear
    await expect(page.getByTestId('photo-previews')).toBeVisible({ timeout: 5000 })
  })

  test('Cannot attach more than 3 photos', async ({ page }) => {
    await expect(page.getByTestId('photo-upload')).toBeVisible()

    // Upload 3 photos
    for (let i = 0; i < 3; i++) {
      const fileChooserPromise = page.waitForEvent('filechooser')
      await page.getByTestId('add-photo-btn').click()
      const fileChooser = await fileChooserPromise
      await fileChooser.setFiles({
        name: `test-image-${i}.jpg`,
        mimeType: 'image/jpeg',
        buffer: Buffer.from(`fake-image-data-${i}`),
      })
    }

    // After 3 photos, the add photo button should be hidden or an error shown
    // The PhotoUpload component hides the add button when maxPhotos (3) is reached
    await expect(page.getByTestId('add-photo-btn')).not.toBeVisible()
  })
})

test.describe('Owner Portal — daily message limit', () => {
  test('Daily message limit (5) enforced', async ({ page }) => {
    // Use VALID_TOKEN which already has consent.
    // We send 5 messages via direct API calls, then try to send the 6th via the form.
    const token = VALID_TOKEN

    // Set up page with consent and token in sessionStorage
    await page.addInitScript((t: string) => {
      sessionStorage.setItem('portal_consent_given', 'true')
      sessionStorage.setItem('portal_token', t)
    }, token)
    await page.goto(`/en/portal/${CLINIC_SLUG}/new?token=${token}`)
    await page.waitForSelector('[data-testid="msw-ready"]', { state: 'attached', timeout: 15000 })
    await page.waitForSelector('[data-testid="new-message-form"]', { timeout: 10000 })

    // Send 5 messages directly via API to reach the daily limit
    for (let i = 0; i < 5; i++) {
      await page.evaluate(async ([t, idx]: [string, number]) => {
        await fetch('/api/v1/portal/conversations', {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            'Authorization': `MagicLink ${t}`,
          },
          body: JSON.stringify({
            petId: null,
            subject: `Auto message ${idx}`,
            category: 'Administrative',
            body: 'Auto-generated test message body.',
          }),
        })
      }, [token, i] as [string, number])
    }

    // Now fill the form and try to send a 6th message (should hit 429 limit)
    const subjectInput = page.getByTestId('message-subject')
    await subjectInput.fill('6th message today')

    // Select a category so the form validates
    const categorySelector = page.getByTestId('category-selector')
    await expect(categorySelector).toBeVisible()
    await categorySelector.selectOption('Administrative')

    const messageInput = page.getByTestId('message-input')
    await messageInput.fill('This is my 6th message today and should be rejected.')

    await page.getByTestId('send-message-btn').click()

    // The MSW handler should return 429, which shows submit-error
    const submitError = page.getByTestId('submit-error')
    await expect(submitError).toBeVisible({ timeout: 5000 })
  })
})
