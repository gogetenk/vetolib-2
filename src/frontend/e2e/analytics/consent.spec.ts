import { test, expect, type Page } from "@playwright/test";

// Helper: log in via the login form (MSW mock)
async function doLogin(page: Page) {
  await page.goto("/en/login");
  await page.waitForSelector('[data-testid="msw-ready"]', {
    state: "attached",
    timeout: 30000,
  });
  await page.getByTestId("email-input").fill("admin@desertpaws.ae");
  await page.getByTestId("password-input").fill("Admin123!");
  await page.getByTestId("signin-button").click();
  await page.waitForURL("**/dashboard**", { timeout: 15000 });
}

test.describe("Analytics consent", () => {
  test.beforeEach(async ({ context }) => {
    // Clear localStorage before each test to reset consent state
    await context.addInitScript(() => {
      localStorage.removeItem("analytics_consent");
    });
  });

  test("consent banner appears on first visit", async ({ page }) => {
    await doLogin(page);
    await expect(page.getByTestId("consent-banner")).toBeVisible();
  });

  test("consent banner disappears after accepting", async ({ page }) => {
    await doLogin(page);
    await expect(page.getByTestId("consent-banner")).toBeVisible();
    await page.getByTestId("consent-accept").click();
    await expect(page.getByTestId("consent-banner")).not.toBeVisible();
  });

  test("consent banner disappears after declining", async ({ page }) => {
    await doLogin(page);
    await expect(page.getByTestId("consent-banner")).toBeVisible();
    await page.getByTestId("consent-decline").click();
    await expect(page.getByTestId("consent-banner")).not.toBeVisible();
  });

  test("consent banner does not reappear after choice", async ({ page }) => {
    await doLogin(page);
    await page.getByTestId("consent-accept").click();
    await expect(page.getByTestId("consent-banner")).not.toBeVisible();

    // Reload the page
    await page.reload();
    await page.waitForSelector('[data-testid="dashboard-main"]', {
      timeout: 15000,
    });

    // Banner should not be visible since consent was already set
    await expect(page.getByTestId("consent-banner")).not.toBeVisible();
  });

  test("analytics toggle in settings reflects consent state — granted", async ({
    page,
    context,
  }) => {
    // Pre-set consent to 'granted'
    await context.addInitScript(() => {
      localStorage.setItem("analytics_consent", "granted");
    });

    await doLogin(page);
    await page.goto("/en/settings/preferences");
    await page.waitForSelector('[data-testid="preferences-page"]', {
      timeout: 15000,
    });

    const toggle = page.getByTestId("analytics-consent-toggle");
    await expect(toggle).toBeChecked();
  });

  test("analytics toggle in settings reflects consent state — denied", async ({
    page,
    context,
  }) => {
    // Pre-set consent to 'denied'
    await context.addInitScript(() => {
      localStorage.setItem("analytics_consent", "denied");
    });

    await doLogin(page);
    await page.goto("/en/settings/preferences");
    await page.waitForSelector('[data-testid="preferences-page"]', {
      timeout: 15000,
    });

    const toggle = page.getByTestId("analytics-consent-toggle");
    await expect(toggle).not.toBeChecked();
  });
});
