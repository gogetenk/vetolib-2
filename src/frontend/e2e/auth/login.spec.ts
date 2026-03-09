import { test, expect, Page } from "@playwright/test";

// Page Object Model for the login page
class LoginPage {
  constructor(private page: Page) {}

  async goto() {
    await this.page.goto("/login");
    // Wait for MSW service worker to activate before interacting
    await this.page.waitForSelector('[data-testid="msw-ready"]', {
      state: "attached",
      timeout: 60000,
    });
  }

  async fillEmail(email: string) {
    await this.page.getByTestId("email-input").fill(email);
  }

  async fillPassword(password: string) {
    await this.page.getByTestId("password-input").fill(password);
  }

  async submit() {
    await this.page.getByTestId("signin-button").click();
  }

  async getError() {
    return this.page.getByTestId("error-message");
  }

  async loginWith(email: string, password: string) {
    await this.fillEmail(email);
    await this.fillPassword(password);
    await this.submit();
  }
}

test.describe("Login page", () => {
  test.beforeEach(async ({ page }) => {
    // Clear auth state before each test — no need to wait for MSW here
    await page.goto("/login");
    await page.waitForLoadState("domcontentloaded");
    await page.evaluate(() => {
      localStorage.removeItem("access_token");
      localStorage.removeItem("refresh_token");
      document.cookie =
        "access_token=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;";
    });
  });

  test("Successful login redirects to appointments page", async ({ page }) => {
    const loginPage = new LoginPage(page);
    await loginPage.goto();

    await loginPage.loginWith("dr.sarah@desertpaws.ae", "Secure123!");

    // Should redirect to appointments
    await expect(page).toHaveURL(/\/appointments/);

    // Should show the clinic name in header
    await expect(page.getByTestId("clinic-name")).toContainText(
      "Desert Paws Clinic"
    );
  });

  test("Invalid password shows error message", async ({ page }) => {
    const loginPage = new LoginPage(page);
    await loginPage.goto();

    await loginPage.loginWith("dr.sarah@desertpaws.ae", "wrongpassword");

    const error = await loginPage.getError();
    await expect(error).toBeVisible();
    await expect(error).toHaveText("Invalid email or password");

    // Should remain on login page
    await expect(page).toHaveURL(/\/login/);
  });

  test("Unknown email shows same generic error (no user enumeration)", async ({
    page,
  }) => {
    const loginPage = new LoginPage(page);
    await loginPage.goto();

    await loginPage.loginWith("unknown@example.com", "anything");

    const error = await loginPage.getError();
    await expect(error).toBeVisible();
    await expect(error).toHaveText("Invalid email or password");
  });

  test("Account locked after 5 failed attempts", async ({ page }) => {
    const loginPage = new LoginPage(page);
    await loginPage.goto();

    // Make 5 failed attempts
    for (let i = 0; i < 5; i++) {
      await loginPage.fillEmail("dr.sarah@desertpaws.ae");
      await loginPage.fillPassword("wrongpassword");
      await loginPage.submit();
      // Wait for the error to appear before next attempt
      await page.getByTestId("error-message").waitFor({ state: "visible" });
    }

    // 5th attempt triggers lock
    await expect(page.getByTestId("error-message")).toHaveText(
      "Account locked. Try again in 15 minutes."
    );

    // 6th attempt with correct password still locked
    await loginPage.fillEmail("dr.sarah@desertpaws.ae");
    await loginPage.fillPassword("Secure123!");
    await loginPage.submit();

    await expect(page.getByTestId("error-message")).toHaveText(
      "Account locked. Try again in 15 minutes."
    );
  });

  test("Empty fields show client-side validation without API call", async ({
    page,
  }) => {
    const loginPage = new LoginPage(page);
    await loginPage.goto();

    // Track network requests to /api/auth/login
    const apiRequests: string[] = [];
    page.on("request", (req) => {
      if (req.url().includes("/api/auth/login")) {
        apiRequests.push(req.url());
      }
    });

    // Click sign in without filling fields
    await loginPage.submit();

    // Should show validation errors
    const error = await loginPage.getError();
    await expect(error).toBeVisible();

    // No API call should have been made
    expect(apiRequests).toHaveLength(0);

    // Should remain on login page
    await expect(page).toHaveURL(/\/login/);
  });

  test("Already authenticated user visiting /login redirects to /appointments", async ({
    page,
  }) => {
    // First login successfully
    const loginPage = new LoginPage(page);
    await loginPage.goto();
    await loginPage.loginWith("dr.sarah@desertpaws.ae", "Secure123!");
    await expect(page).toHaveURL(/\/appointments/);

    // Now try to go back to /login — middleware should redirect
    await page.goto("/login");
    // The middleware checks the cookie — should redirect to appointments
    await expect(page).toHaveURL(/\/appointments/);
  });

  test("Sign Out clears session and redirects to login", async ({ page }) => {
    // Login first
    const loginPage = new LoginPage(page);
    await loginPage.goto();
    await loginPage.loginWith("dr.sarah@desertpaws.ae", "Secure123!");
    await expect(page).toHaveURL(/\/appointments/);

    // Click user menu trigger
    await page.getByTestId("user-menu-trigger").click();

    // Click sign out
    await page.getByTestId("user-menu-signout").click();

    // Should redirect to login
    await expect(page).toHaveURL(/\/login/);

    // Navigating to /appointments should redirect back to login
    await page.goto("/appointments");
    await expect(page).toHaveURL(/\/login/);
  });
});
