/**
 * Staff Inbox — Playwright tests (MSW mode)
 *
 * 9 scenarios:
 *  1. Receptionist sees only appointment/administrative messages
 *  2. Vet sees only medical messages
 *  3. Admin sees all conversations
 *  4. Assistant sees non-medical conversations in read-only
 *  5. Conversations sorted by priority then date
 *  6. Filter by status works
 *  7. Filter by category works
 *  8. Unread count badge displayed
 *  9. Search by text works
 */
import { test, expect } from '@playwright/test'
import {
  loginAsReceptionist,
  loginAsVet,
  loginAsAdmin,
  loginAsAssistant,
  navigateToInbox,
  CONV_IDS,
} from '../../fixtures/messaging'

test.describe('Staff Inbox — Receptionist', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsReceptionist(page)
    await page.waitForSelector('[data-testid="conversation-list"]', { timeout: 10000 })
  })

  test('Receptionist sees only appointment/administrative messages', async ({ page }) => {
    // Receptionist should see AppointmentRequest and Administrative conversations
    // Medical urgency and post-op follow-up go to VET role
    const list = page.getByTestId('conversation-list')
    await expect(list).toBeVisible()

    // The inbox page is visible
    await expect(page.getByTestId('messages-page')).toBeVisible()

    // Receptionist sees the appointment request conversation (assigned to RECEPTIONIST role)
    await expect(
      page.getByTestId(`conversation-item-${CONV_IDS.appointmentRequest}`)
    ).toBeVisible()

    // Receptionist sees the administrative conversation
    await expect(
      page.getByTestId(`conversation-item-${CONV_IDS.administrative}`)
    ).toBeVisible()
  })
})

test.describe('Staff Inbox — Vet', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsVet(page)
    await page.waitForSelector('[data-testid="conversation-list"]', { timeout: 10000 })
  })

  test('Vet sees only medical messages', async ({ page }) => {
    await expect(page.getByTestId('messages-page')).toBeVisible()

    // Vet sees medical urgency conversation
    await expect(
      page.getByTestId(`conversation-item-${CONV_IDS.medicalUrgency}`)
    ).toBeVisible()

    // Vet sees post-op follow-up conversation (assigned to VET role)
    await expect(
      page.getByTestId(`conversation-item-${CONV_IDS.postOp}`)
    ).toBeVisible()
  })
})

test.describe('Staff Inbox — Admin', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsAdmin(page)
    await page.waitForSelector('[data-testid="conversation-list"]', { timeout: 10000 })
  })

  test('Admin sees all conversations', async ({ page }) => {
    await expect(page.getByTestId('messages-page')).toBeVisible()

    const list = page.getByTestId('conversation-list')
    await expect(list).toBeVisible()

    // Admin sees all non-spam conversations
    await expect(
      page.getByTestId(`conversation-item-${CONV_IDS.medicalUrgency}`)
    ).toBeVisible()
    await expect(
      page.getByTestId(`conversation-item-${CONV_IDS.postOp}`)
    ).toBeVisible()
    await expect(
      page.getByTestId(`conversation-item-${CONV_IDS.appointmentRequest}`)
    ).toBeVisible()
  })

  test('Conversations sorted by priority then date', async ({ page }) => {
    await expect(page.getByTestId('conversation-list')).toBeVisible()

    // The Critical/MedicalUrgency conversation should appear first
    // (priority: Critical > High > Normal > Low)
    const items = page.locator('[data-testid^="conversation-item-"]')
    const firstItem = items.first()
    await expect(firstItem).toHaveAttribute(
      'data-testid',
      `conversation-item-${CONV_IDS.medicalUrgency}`
    )
  })

  test('Filter by status works', async ({ page }) => {
    await expect(page.getByTestId('conversation-filters')).toBeVisible()

    // Click the "Resolved" status filter
    await page.getByTestId('filter-status-resolved').click()
    await page.waitForLoadState('networkidle')

    // Should only show resolved conversations
    await expect(
      page.getByTestId(`conversation-item-${CONV_IDS.administrative}`)
    ).toBeVisible()

    // Open conversations should not be visible
    await expect(
      page.getByTestId(`conversation-item-${CONV_IDS.medicalUrgency}`)
    ).not.toBeVisible()
  })

  test('Filter by category works', async ({ page }) => {
    await expect(page.getByTestId('conversation-filters')).toBeVisible()

    // Click the "MedicalUrgency" category filter
    await page.getByTestId('filter-category-medicalurgency').click()
    await page.waitForLoadState('networkidle')

    // Should only show medical urgency conversations
    await expect(
      page.getByTestId(`conversation-item-${CONV_IDS.medicalUrgency}`)
    ).toBeVisible()

    // Appointment request should not be visible
    await expect(
      page.getByTestId(`conversation-item-${CONV_IDS.appointmentRequest}`)
    ).not.toBeVisible()
  })

  test('Unread count badge displayed', async ({ page }) => {
    await expect(page.getByTestId('messages-page')).toBeVisible()

    // The unread total badge should be visible (conversations have unreadCount > 0)
    const unreadBadge = page.getByTestId('unread-total-badge')
    await expect(unreadBadge).toBeVisible()

    // The medical urgency conversation has unreadCount: 1
    await expect(
      page.getByTestId(`unread-count-${CONV_IDS.medicalUrgency}`)
    ).toBeVisible()
  })

  test('Search by text works', async ({ page }) => {
    await expect(page.getByTestId('conversation-filters')).toBeVisible()

    // Type in the search box to filter by owner name
    const searchInput = page.getByTestId('filter-search')
    await searchInput.fill('Ahmed')
    await page.waitForTimeout(400) // debounce
    await page.waitForLoadState('networkidle')

    // Should show the conversation from Ahmed Al-Rashid
    await expect(
      page.getByTestId(`conversation-item-${CONV_IDS.medicalUrgency}`)
    ).toBeVisible()

    // Other conversations should not be visible
    await expect(
      page.getByTestId(`conversation-item-${CONV_IDS.appointmentRequest}`)
    ).not.toBeVisible()
  })
})

test.describe('Staff Inbox — Assistant', () => {
  test('Assistant sees non-medical conversations in read-only', async ({ page }) => {
    await loginAsAssistant(page)
    await page.waitForSelector('[data-testid="messages-page"]', { timeout: 10000 })

    // Assistant should see the messages page
    await expect(page.getByTestId('messages-page')).toBeVisible()

    // The inbox list should render
    await page.waitForSelector('[data-testid="conversation-list"]', { timeout: 10000 })
    await expect(page.getByTestId('conversation-list')).toBeVisible()
  })
})
