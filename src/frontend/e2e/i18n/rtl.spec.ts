import { test, expect } from '@playwright/test'

/**
 * i18n / RTL tests
 * These run against the MSW dev server (next dev:webpack).
 * No authentication required for the RTL/lang attribute checks on /login.
 */

test.describe('RTL and locale', () => {
  test('English layout has dir=ltr', async ({ page }) => {
    await page.goto('/en/login')
    const html = page.locator('html')
    await expect(html).toHaveAttribute('dir', 'ltr')
    await expect(html).toHaveAttribute('lang', 'en')
  })

  test('Arabic layout has dir=rtl', async ({ page }) => {
    await page.goto('/ar/login')
    const html = page.locator('html')
    await expect(html).toHaveAttribute('dir', 'rtl')
    await expect(html).toHaveAttribute('lang', 'ar')
  })

  test('Arabic login page shows Arabic text', async ({ page }) => {
    await page.goto('/ar/login')
    // The card description should show Arabic veterinary management text
    await expect(page.getByText('إدارة العيادة البيطرية')).toBeVisible()
  })

  test('English login page shows English text', async ({ page }) => {
    await page.goto('/en/login')
    await expect(page.getByText('Veterinary Management')).toBeVisible()
  })
})

test.describe('Language switcher', () => {
  test('Language switcher changes locale on appointments page', async ({ page }) => {
    // First log in
    await page.goto('/en/login')
    await page.getByTestId('email-input').fill('dr.ahmed@vets.ae')
    await page.getByTestId('password-input').fill('password123')
    await page.getByTestId('signin-button').click()
    await page.waitForURL('**/en/appointments')

    // Switch to Arabic
    await page.getByTestId('lang-switcher-ar').click()
    await page.waitForURL('**/ar/appointments')

    const html = page.locator('html')
    await expect(html).toHaveAttribute('dir', 'rtl')
    await expect(html).toHaveAttribute('lang', 'ar')

    // Page title should be in Arabic
    const title = page.getByTestId('appointments-title')
    await expect(title).toContainText('المواعيد')
  })

  test('Language switcher switches back from Arabic to English', async ({ page }) => {
    await page.goto('/en/login')
    await page.getByTestId('email-input').fill('dr.ahmed@vets.ae')
    await page.getByTestId('password-input').fill('password123')
    await page.getByTestId('signin-button').click()
    await page.waitForURL('**/en/appointments')

    // Switch to Arabic first
    await page.getByTestId('lang-switcher-ar').click()
    await page.waitForURL('**/ar/appointments')

    // Switch back to English
    await page.getByTestId('lang-switcher-en').click()
    await page.waitForURL('**/en/appointments')

    const html = page.locator('html')
    await expect(html).toHaveAttribute('dir', 'ltr')
    await expect(html).toHaveAttribute('lang', 'en')
  })
})
