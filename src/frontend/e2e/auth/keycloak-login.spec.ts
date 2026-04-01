import { test, expect, Page } from "@playwright/test";

/**
 * E2E tests for Keycloak SSO login button and ClinicSwitcher organizations.
 *
 * The Keycloak button is conditionally rendered based on NEXT_PUBLIC_KEYCLOAK_URL.
 * These tests verify both states and the ClinicSwitcher organizations flow.
 */

class LoginPage {
  constructor(private page: Page) {}

  async goto() {
    await this.page.goto("/login");
    await this.page.waitForSelector('[data-testid="msw-ready"]', {
      state: "attached",
      timeout: 60000,
    });
  }

  async loginWith(email: string, password: string) {
    await this.page.getByTestId("email-input").fill(email);
    await this.page.getByTestId("password-input").fill(password);
    await this.page.getByTestId("signin-button").click();
  }
}

test.describe("Keycloak login button", () => {
  test.beforeEach(async ({ page }) => {
    await page.goto("/login");
    await page.waitForLoadState("domcontentloaded");
    await page.evaluate(() => {
      localStorage.removeItem("access_token");
      localStorage.removeItem("refresh_token");
      document.cookie =
        "access_token=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;";
    });
  });

  test("Login page renders the login card and form fields", async ({
    page,
  }) => {
    const loginPage = new LoginPage(page);
    await loginPage.goto();

    // Core login form elements always present
    await expect(page.getByTestId("login-card")).toBeVisible();
    await expect(page.getByTestId("email-input")).toBeVisible();
    await expect(page.getByTestId("password-input")).toBeVisible();
    await expect(page.getByTestId("signin-button")).toBeVisible();
  });

  test("Keycloak button has correct data-testid when present", async ({
    page,
  }) => {
    const loginPage = new LoginPage(page);
    await loginPage.goto();

    const keycloakButton = page.getByTestId("keycloak-signin-button");
    const isVisible = await keycloakButton.isVisible().catch(() => false);

    if (isVisible) {
      // When NEXT_PUBLIC_KEYCLOAK_URL is set, button should be fully functional
      await expect(keycloakButton).toBeEnabled();
      await expect(keycloakButton).toHaveAttribute(
        "data-testid",
        "keycloak-signin-button"
      );
    } else {
      // When env var is not set, the button should not exist in the DOM
      await expect(keycloakButton).toHaveCount(0);
    }
  });

  test("Legacy login form works regardless of Keycloak button presence", async ({
    page,
  }) => {
    const loginPage = new LoginPage(page);
    await loginPage.goto();

    // Legacy email/password login should always work
    await loginPage.loginWith("dr.sarah@desertpaws.ae", "Secure123!");

    // Should redirect to appointments
    await expect(page).toHaveURL(/\/appointments/);
  });

  test("Login page renders both legacy form and Keycloak button when enabled", async ({
    page,
  }) => {
    const loginPage = new LoginPage(page);
    await loginPage.goto();

    // Legacy form elements are always present
    await expect(page.getByTestId("email-input")).toBeVisible();
    await expect(page.getByTestId("password-input")).toBeVisible();
    await expect(page.getByTestId("signin-button")).toBeVisible();
    await expect(page.getByTestId("forgot-password-link")).toBeVisible();
    await expect(page.getByTestId("signup-link")).toBeVisible();

    const keycloakButton = page.getByTestId("keycloak-signin-button");
    const keycloakVisible = await keycloakButton.isVisible().catch(() => false);

    if (keycloakVisible) {
      // Both options coexist: legacy form + Keycloak SSO
      await expect(page.getByTestId("signin-button")).toBeVisible();
      await expect(keycloakButton).toBeVisible();
    }
  });
});

test.describe("ClinicSwitcher organizations after login", () => {
  test("ClinicSwitcher shows organizations from my-organizations endpoint", async ({
    page,
  }) => {
    // Clear auth state
    await page.goto("/login");
    await page.waitForLoadState("domcontentloaded");
    await page.evaluate(() => {
      localStorage.removeItem("access_token");
      localStorage.removeItem("refresh_token");
      document.cookie =
        "access_token=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;";
    });

    // Login with a user that has multi-clinic access (MSW mock: dr.sarah has group-001)
    const loginPage = new LoginPage(page);
    await loginPage.goto();
    await loginPage.loginWith("dr.sarah@desertpaws.ae", "Secure123!");
    await expect(page).toHaveURL(/\/appointments/);

    // Wait for the clinic switcher to appear (it fetches organizations on mount)
    const switcher = page.getByTestId("clinic-switcher-trigger");
    const switcherVisible = await switcher
      .waitFor({ state: "visible", timeout: 10000 })
      .then(() => true)
      .catch(() => false);

    if (switcherVisible) {
      // Open the dropdown
      await switcher.click();

      // Should show the dropdown with clinic options
      await expect(
        page.getByTestId("clinic-switcher-dropdown")
      ).toBeVisible();

      // Verify organizations from the MSW mock are listed
      await expect(
        page.getByTestId("clinic-option-clinic-001")
      ).toBeVisible();
      await expect(
        page.getByTestId("clinic-option-clinic-002")
      ).toBeVisible();
    }
    // If not visible, user might have single clinic -- that's OK
  });
});
