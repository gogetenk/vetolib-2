/**
 * Wire e2e tests — run against the real ASP.NET Core backend.
 * These tests are tagged @integration and run via:
 *   npm run test:e2e:integration
 *
 * Seed credentials (from DbInitializer.cs):
 *   admin@desertpaws.ae / Admin123!
 *   dr.sarah@desertpaws.ae / Vet12345!
 *
 * Note: In wire mode, NEXT_PUBLIC_API_URL is set, so MSW is disabled.
 * The msw-ready sentinel is still rendered (mswReady defaults to true when
 * MSW is skipped), so existing page-object patterns still work.
 */
import { test, expect, type Page } from "@playwright/test";

// Seed credentials from DbInitializer.cs
const ADMIN_EMAIL = "admin@desertpaws.ae";
const ADMIN_PASSWORD = "Admin123!";
const VET_EMAIL = "dr.sarah@desertpaws.ae";
const VET_PASSWORD = "Vet12345!";

/** Login via the UI and return when the appointments page is visible. */
async function loginViaUI(
  page: Page,
  email: string,
  password: string
): Promise<void> {
  await page.goto("/login");
  // Wait for MSW-ready sentinel (also present in wire mode, renders immediately)
  await page.waitForSelector('[data-testid="msw-ready"]', {
    state: "attached",
    timeout: 30000,
  });
  await page.getByTestId("email-input").fill(email);
  await page.getByTestId("password-input").fill(password);
  await page.getByTestId("signin-button").click();
  await expect(page).toHaveURL(/\/appointments/, { timeout: 15000 });
}

// ─────────────────────────────────────────────────────────────────────────────
// Auth
// ─────────────────────────────────────────────────────────────────────────────
test.describe("@integration Auth — real backend", () => {
  test("admin login succeeds and redirects to appointments", async ({
    page,
  }) => {
    await loginViaUI(page, ADMIN_EMAIL, ADMIN_PASSWORD);
    await expect(page.getByTestId("clinic-name")).toBeVisible({ timeout: 10000 });
  });

  test("vet login succeeds", async ({ page }) => {
    await loginViaUI(page, VET_EMAIL, VET_PASSWORD);
    await expect(page.getByTestId("clinic-name")).toBeVisible({ timeout: 10000 });
  });

  test("wrong password shows error", async ({ page }) => {
    await page.goto("/login");
    await page.waitForSelector('[data-testid="msw-ready"]', {
      state: "attached",
      timeout: 30000,
    });
    await page.getByTestId("email-input").fill(ADMIN_EMAIL);
    await page.getByTestId("password-input").fill("WrongPassword!");
    await page.getByTestId("signin-button").click();

    await expect(page.getByTestId("error-message")).toBeVisible({
      timeout: 10000,
    });
    await expect(page).toHaveURL(/\/login/);
  });
});

// ─────────────────────────────────────────────────────────────────────────────
// Patients CRUD
// ─────────────────────────────────────────────────────────────────────────────
test.describe("@integration Patients — real backend", () => {
  test.beforeEach(async ({ page }) => {
    await loginViaUI(page, VET_EMAIL, VET_PASSWORD);
  });

  test("patient list loads (at least 0 results — no hardcoded count)", async ({
    page,
  }) => {
    await page.goto("/patients");
    await page.waitForSelector('[data-testid="patients-page"]', {
      timeout: 15000,
    });
    // Just verify the page renders — count depends on DB state
    await expect(page.getByTestId("patients-page")).toBeVisible();
  });

  test("can create a patient and see it in the list", async ({ page }) => {
    await page.goto("/patients/new");
    await page.waitForSelector('[data-testid="patient-form"]', {
      timeout: 15000,
    });

    const uniqueName = `Wire-Dog-${Date.now()}`;
    await page.getByTestId("input-patient-name").fill(uniqueName);
    await page.getByTestId("select-species-trigger").click();
    await page.getByTestId("species-option-dog").click();
    await page.getByTestId("input-breed").fill("Labrador");
    await page.getByTestId("input-date-of-birth").fill("2022-06-15");
    await page.getByTestId("select-gender-trigger").click();
    await page.getByTestId("gender-option-male").click();
    await page.getByTestId("input-owner-name").fill("Test Owner UAE");
    await page.getByTestId("input-owner-phone").fill("+971 50 000 1111");
    await page.getByTestId("btn-save-patient").click();

    // Redirected to patient detail page
    await expect(page).toHaveURL(/\/patients\/[a-z0-9-]+$/, { timeout: 10000 });
    await page.waitForSelector('[data-testid="patient-detail-page"]', {
      timeout: 10000,
    });
    await expect(page.getByTestId("patient-detail-name")).toContainText(
      uniqueName
    );
  });
});

// ─────────────────────────────────────────────────────────────────────────────
// Dashboard
// ─────────────────────────────────────────────────────────────────────────────
test.describe("@integration Dashboard — real backend", () => {
  test("dashboard renders with real stats", async ({ page }) => {
    await loginViaUI(page, ADMIN_EMAIL, ADMIN_PASSWORD);
    await page.goto("/");
    await page.waitForSelector('[data-testid="msw-ready"]', {
      state: "attached",
      timeout: 15000,
    });
    // Dashboard should render without throwing — real counts may be 0
    await expect(page).not.toHaveURL(/\/login/);
  });
});

// ─────────────────────────────────────────────────────────────────────────────
// Appointments
// ─────────────────────────────────────────────────────────────────────────────
test.describe("@integration Appointments — real backend", () => {
  test("appointments page loads", async ({ page }) => {
    await loginViaUI(page, VET_EMAIL, VET_PASSWORD);
    await expect(page).toHaveURL(/\/appointments/, { timeout: 10000 });
    // Page should be visible — no hardcoded count assertions
    await page.waitForSelector('[data-testid="msw-ready"]', {
      state: "attached",
      timeout: 15000,
    });
    await expect(page).not.toHaveURL(/\/login/);
  });
});

// ─────────────────────────────────────────────────────────────────────────────
// Billing
// ─────────────────────────────────────────────────────────────────────────────
test.describe("@integration Billing — real backend", () => {
  test("billing page loads", async ({ page }) => {
    await loginViaUI(page, VET_EMAIL, VET_PASSWORD);
    await page.goto("/billing");
    await page.waitForSelector('[data-testid="msw-ready"]', {
      state: "attached",
      timeout: 15000,
    });
    await expect(page).not.toHaveURL(/\/login/);
  });
});
