import { test, expect, Page } from "@playwright/test";

const SCREENSHOT_DIR = "e2e/screenshots/dashboard";

// Run serially — these are screenshot captures, not parallel tests
test.describe.configure({ mode: "serial" });

// Increase timeout for screenshot captures (server startup + rendering)
test.setTimeout(120000);

async function loginAs(page: Page, email: string, password: string) {
  await page.goto("/en/login", { timeout: 60000 });
  await page.waitForSelector('[data-testid="msw-ready"]', {
    state: "attached",
    timeout: 60000,
  });
  await page.getByTestId("email-input").fill(email);
  await page.getByTestId("password-input").fill(password);
  await page.getByTestId("signin-button").click();
  await page.waitForURL((url) => !url.pathname.includes("/login"), {
    timeout: 15000,
  });
}

async function clearAuth(page: Page) {
  await page.goto("/en/login", { timeout: 60000 });
  await page.waitForLoadState("domcontentloaded");
  await page.evaluate(() => {
    localStorage.removeItem("access_token");
    localStorage.removeItem("refresh_token");
    document.cookie =
      "access_token=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;";
  });
}

test.describe("Dashboard & Appointments Screenshots", () => {
  test.beforeEach(async ({ page }) => {
    await clearAuth(page);
  });

  test("01 - Dashboard main view", async ({ page }) => {
    await loginAs(page, "dr.sarah@desertpaws.ae", "Secure123!");
    await page.goto("/en/dashboard", { timeout: 60000 });
    await page.waitForSelector('[data-testid="msw-ready"]', {
      state: "attached",
      timeout: 60000,
    });
    try {
      await page.waitForSelector('[data-testid="dashboard-home"]', {
        timeout: 10000,
      });
    } catch {
      await page.waitForTimeout(3000);
    }
    await page.screenshot({
      path: `${SCREENSHOT_DIR}/01-dashboard.png`,
      fullPage: true,
    });
  });

  test("02 - Dashboard mobile view", async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 812 });
    await loginAs(page, "dr.sarah@desertpaws.ae", "Secure123!");
    await page.goto("/en/dashboard", { timeout: 60000 });
    await page.waitForSelector('[data-testid="msw-ready"]', {
      state: "attached",
      timeout: 60000,
    });
    try {
      await page.waitForSelector('[data-testid="dashboard-home"]', {
        timeout: 10000,
      });
    } catch {
      await page.waitForTimeout(3000);
    }
    await page.screenshot({
      path: `${SCREENSHOT_DIR}/02-dashboard-mobile.png`,
      fullPage: true,
    });
  });

  test("03 - Appointments list", async ({ page }) => {
    await loginAs(page, "dr.sarah@desertpaws.ae", "Secure123!");
    await page.goto("/en/appointments", { timeout: 60000 });
    await page.waitForSelector('[data-testid="msw-ready"]', {
      state: "attached",
      timeout: 60000,
    });
    await page.waitForTimeout(3000);
    await page.screenshot({
      path: `${SCREENSHOT_DIR}/03-appointments-list.png`,
      fullPage: true,
    });
  });

  test("04 - Appointments list mobile", async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 812 });
    await loginAs(page, "dr.sarah@desertpaws.ae", "Secure123!");
    await page.goto("/en/appointments", { timeout: 60000 });
    await page.waitForSelector('[data-testid="msw-ready"]', {
      state: "attached",
      timeout: 60000,
    });
    await page.waitForTimeout(3000);
    await page.screenshot({
      path: `${SCREENSHOT_DIR}/04-appointments-list-mobile.png`,
      fullPage: true,
    });
  });

  test("05 - New appointment form", async ({ page }) => {
    await loginAs(page, "dr.sarah@desertpaws.ae", "Secure123!");
    await page.goto("/en/appointments/new", { timeout: 60000 });
    await page.waitForSelector('[data-testid="msw-ready"]', {
      state: "attached",
      timeout: 60000,
    });
    await page.waitForTimeout(3000);
    await page.screenshot({
      path: `${SCREENSHOT_DIR}/05-appointment-new.png`,
      fullPage: true,
    });
  });

  test("06 - New appointment form filled", async ({ page }) => {
    await loginAs(page, "dr.sarah@desertpaws.ae", "Secure123!");
    await page.goto("/en/appointments/new", { timeout: 60000 });
    await page.waitForSelector('[data-testid="msw-ready"]', {
      state: "attached",
      timeout: 60000,
    });
    await page.waitForTimeout(3000);

    // Try to fill fields
    const inputs = page.locator('input[type="text"], input:not([type])');
    const inputCount = await inputs.count();
    if (inputCount > 0) {
      await inputs.first().fill("Max").catch(() => {});
    }
    const textareas = page.locator("textarea");
    if ((await textareas.count()) > 0) {
      await textareas
        .first()
        .fill("Annual vaccination checkup")
        .catch(() => {});
    }
    const selects = page.locator("select");
    if ((await selects.count()) > 0) {
      await selects.first().selectOption({ index: 1 }).catch(() => {});
    }

    await page.screenshot({
      path: `${SCREENSHOT_DIR}/06-appointment-new-filled.png`,
      fullPage: true,
    });
  });

  test("07 - Appointment detail", async ({ page }) => {
    await loginAs(page, "dr.sarah@desertpaws.ae", "Secure123!");
    await page.goto("/en/appointments", { timeout: 60000 });
    await page.waitForSelector('[data-testid="msw-ready"]', {
      state: "attached",
      timeout: 60000,
    });
    await page.waitForTimeout(3000);

    const appointmentLink = page
      .locator(
        '[data-testid^="appointment-link-"], [data-testid^="appointment-row-"], a[href*="/appointments/"]'
      )
      .first();

    if ((await appointmentLink.count()) > 0) {
      await appointmentLink.click();
      await page.waitForTimeout(3000);
    } else {
      await page.goto(
        "/en/appointments/a1b2c3d4-0000-0000-0000-000000000001",
        { timeout: 60000 }
      );
      await page.waitForTimeout(3000);
    }

    await page.screenshot({
      path: `${SCREENSHOT_DIR}/07-appointment-detail.png`,
      fullPage: true,
    });
  });

  test("08 - Appointment detail actions", async ({ page }) => {
    await loginAs(page, "dr.sarah@desertpaws.ae", "Secure123!");
    await page.goto("/en/appointments", { timeout: 60000 });
    await page.waitForSelector('[data-testid="msw-ready"]', {
      state: "attached",
      timeout: 60000,
    });
    await page.waitForTimeout(3000);

    const appointmentLink = page
      .locator(
        '[data-testid^="appointment-link-"], [data-testid^="appointment-row-"], a[href*="/appointments/"]'
      )
      .first();

    if ((await appointmentLink.count()) > 0) {
      await appointmentLink.click();
      await page.waitForTimeout(3000);
    } else {
      await page.goto(
        "/en/appointments/a1b2c3d4-0000-0000-0000-000000000001",
        { timeout: 60000 }
      );
      await page.waitForTimeout(3000);
    }

    // Try to open a status transition dialog or actions menu
    const actionButton = page
      .locator(
        '[data-testid*="status"], [data-testid*="action"], button:has-text("Cancel"), button:has-text("Check"), button:has-text("Complete"), button:has-text("Start")'
      )
      .first();

    if ((await actionButton.count()) > 0) {
      await actionButton.click();
      await page.waitForTimeout(1000);
    }

    await page.screenshot({
      path: `${SCREENSHOT_DIR}/08-appointment-detail-actions.png`,
      fullPage: true,
    });
  });

  test("09 - Dashboard Arabic", async ({ page }) => {
    await loginAs(page, "dr.sarah@desertpaws.ae", "Secure123!");
    await page.goto("/ar/dashboard", { timeout: 60000 });
    await page.waitForSelector('[data-testid="msw-ready"]', {
      state: "attached",
      timeout: 60000,
    });
    try {
      await page.waitForSelector('[data-testid="dashboard-home"]', {
        timeout: 10000,
      });
    } catch {
      await page.waitForTimeout(3000);
    }
    await page.screenshot({
      path: `${SCREENSHOT_DIR}/09-dashboard-ar.png`,
      fullPage: true,
    });
  });

  test("10 - Appointments Arabic", async ({ page }) => {
    await loginAs(page, "dr.sarah@desertpaws.ae", "Secure123!");
    await page.goto("/ar/appointments", { timeout: 60000 });
    await page.waitForSelector('[data-testid="msw-ready"]', {
      state: "attached",
      timeout: 60000,
    });
    await page.waitForTimeout(3000);
    await page.screenshot({
      path: `${SCREENSHOT_DIR}/10-appointments-ar.png`,
      fullPage: true,
    });
  });
});
