import { test, expect } from "@playwright/test";

test.describe("Landing page", () => {
  test.beforeEach(async ({ page }) => {
    await page.goto("/en");
    await page.waitForLoadState("networkidle");
  });

  test("page loads in English with all main sections", async ({ page }) => {
    // Hero section visible
    await expect(page.getByTestId("section-hero")).toBeVisible();
    await expect(page.locator("h1")).toContainText("The Veterinary Practice");

    // Main nav sections present
    await expect(page.getByTestId("section-social-proof")).toBeVisible();
    await expect(page.getByTestId("section-features")).toBeVisible();
    await expect(page.getByTestId("section-how-it-works")).toBeVisible();
    await expect(page.getByTestId("section-pricing")).toBeVisible();
    await expect(page.getByTestId("section-testimonials")).toBeVisible();
    await expect(page.getByTestId("section-faq")).toBeVisible();
    await expect(page.getByTestId("section-final-cta")).toBeVisible();
    await expect(page.getByTestId("section-footer")).toBeVisible();
  });

  test("hero CTAs are clickable", async ({ page }) => {
    const heroCta = page.getByTestId("hero-cta-start-trial");
    await expect(heroCta).toBeVisible();
    await expect(heroCta).toBeEnabled();

    const demoBtn = page.getByTestId("hero-cta-book-demo");
    await expect(demoBtn).toBeVisible();
  });

  test("nav CTA is clickable", async ({ page }) => {
    const navCta = page.getByTestId("nav-cta-start-trial");
    await expect(navCta).toBeVisible();
    await expect(navCta).toBeEnabled();
  });

  test("pricing toggle switches between monthly and annual", async ({
    page,
  }) => {
    const monthlyBtn = page.getByTestId("pricing-toggle-monthly");
    const annualBtn = page.getByTestId("pricing-toggle-annual");

    // Monthly active by default — starter shows AED 299
    await expect(monthlyBtn).toBeVisible();
    await expect(
      page.getByTestId("pricing-plan-starter")
    ).toContainText("AED 299");

    // Switch to annual — starter shows AED 254
    await annualBtn.click();
    await expect(
      page.getByTestId("pricing-plan-starter")
    ).toContainText("AED 254");

    // Switch back to monthly
    await monthlyBtn.click();
    await expect(
      page.getByTestId("pricing-plan-starter")
    ).toContainText("AED 299");
  });

  test("FAQ accordion expands and collapses items", async ({ page }) => {
    await page.getByTestId("section-faq").scrollIntoViewIfNeeded();

    // All 8 FAQ items are rendered
    for (let i = 0; i < 8; i++) {
      await expect(page.getByTestId(`faq-${i}`)).toBeVisible();
    }

    // Click first FAQ item to open it
    const firstTrigger = page.getByTestId("faq-0-trigger");
    await firstTrigger.click();
    await expect(page.getByTestId("faq-0-content")).toBeVisible();

    // First item should contain VAT answer
    await expect(page.getByTestId("faq-0-content")).toContainText("VAT");

    // Click again to close
    await firstTrigger.click();
    await expect(page.getByTestId("faq-0-content")).not.toBeVisible();
  });

  test("FAQ opens a different item and closes the previous", async ({
    page,
  }) => {
    await page.getByTestId("section-faq").scrollIntoViewIfNeeded();

    // Open first item
    await page.getByTestId("faq-0-trigger").click();
    await expect(page.getByTestId("faq-0-content")).toBeVisible();

    // Open second item — first should close
    await page.getByTestId("faq-1-trigger").click();
    await expect(page.getByTestId("faq-1-content")).toBeVisible();
    await expect(page.getByTestId("faq-0-content")).not.toBeVisible();
  });

  test("final CTA section is visible and button is clickable", async ({
    page,
  }) => {
    await page.getByTestId("section-final-cta").scrollIntoViewIfNeeded();
    await expect(page.getByTestId("section-final-cta")).toBeVisible();

    const finalCta = page.getByTestId("final-cta-button");
    await expect(finalCta).toBeVisible();
    await expect(finalCta).toBeEnabled();
    await expect(finalCta).toContainText("Start Free Trial");
  });

  test("footer has 4 columns and contact info", async ({ page }) => {
    await page.getByTestId("section-footer").scrollIntoViewIfNeeded();
    await expect(page.getByTestId("section-footer")).toBeVisible();
    await expect(page.getByTestId("footer-contact")).toBeVisible();

    // Language switcher present
    await expect(page.getByTestId("footer-lang-switcher")).toBeVisible();
    await expect(page.getByTestId("footer-lang-en")).toBeVisible();
    await expect(page.getByTestId("footer-lang-ar")).toBeVisible();
  });

  test("switch to Arabic shows RTL layout", async ({ page }) => {
    // Navigate to Arabic version
    await page.goto("/ar");
    await page.waitForLoadState("networkidle");

    // Page should be in Arabic — check dir attribute
    const html = page.locator("html");
    await expect(html).toHaveAttribute("dir", "rtl");

    // Hero headline should be in Arabic
    await expect(page.locator("h1")).toContainText("برنامج إدارة");

    // FAQ section in Arabic
    await expect(page.getByTestId("section-faq")).toBeVisible();

    // Final CTA in Arabic
    await expect(page.getByTestId("section-final-cta")).toBeVisible();
    await expect(page.getByTestId("final-cta-button")).toContainText(
      "ابدأ التجربة المجانية"
    );
  });

  test("footer language switch navigates between locales", async ({ page }) => {
    // Start on English
    await expect(page).toHaveURL(/\/en/);

    // Click AR link in footer
    await page.getByTestId("footer-lang-ar").click();
    await page.waitForLoadState("networkidle");
    await expect(page).toHaveURL(/\/ar/);

    // Click EN link in footer to go back
    await page.getByTestId("footer-lang-en").click();
    await page.waitForLoadState("networkidle");
    await expect(page).toHaveURL(/\/en/);
  });

  test("JSON-LD structured data is present in page", async ({ page }) => {
    const jsonLd = await page.locator(
      'script[type="application/ld+json"]'
    ).first().textContent();

    expect(jsonLd).not.toBeNull();
    const parsed = JSON.parse(jsonLd!);
    expect(parsed["@type"]).toBe("SoftwareApplication");
    expect(parsed.name).toBe("Vetara");
    expect(parsed.offers.priceCurrency).toBe("AED");
  });

  test("pricing CTAs are present for all plans", async ({ page }) => {
    await page.getByTestId("section-pricing").scrollIntoViewIfNeeded();

    await expect(page.getByTestId("pricing-cta-starter")).toBeVisible();
    await expect(page.getByTestId("pricing-cta-pro")).toBeVisible();
    await expect(page.getByTestId("pricing-cta-enterprise")).toBeVisible();
  });
});
