/**
 * Signup page E2E tests — runs against MSW (no backend required)
 *
 * Covers:
 *  - Successful clinic registration → redirect to dashboard + toast
 *  - Email already taken → inline error
 *  - Password validation (strength + match)
 *  - Landing page CTA links point to /signup
 */
import { test, expect } from "@playwright/test";

const SIGNUP_URL = "/en/signup";

async function gotoSignup(page: import("@playwright/test").Page) {
  await page.goto(SIGNUP_URL);
  await page.waitForLoadState("networkidle");
}

async function fillForm(
  page: import("@playwright/test").Page,
  values: {
    clinicName?: string;
    email?: string;
    phone?: string;
    password?: string;
    confirmPassword?: string;
  }
) {
  if (values.clinicName !== undefined) {
    await page.getByTestId("clinic-name-input").fill(values.clinicName);
  }
  if (values.email !== undefined) {
    await page.getByTestId("email-input").fill(values.email);
  }
  if (values.phone !== undefined) {
    await page.getByTestId("phone-input").fill(values.phone);
  }
  if (values.password !== undefined) {
    await page.getByTestId("password-input").fill(values.password);
  }
  if (values.confirmPassword !== undefined) {
    await page.getByTestId("confirm-password-input").fill(values.confirmPassword);
  }
}

// ─────────────────────────────────────────────────────────────────────────────
// SIGNUP-01 : Signup form renders correctly
// ─────────────────────────────────────────────────────────────────────────────
test("SIGNUP-01 signup form renders all fields", async ({ page }) => {
  await gotoSignup(page);

  await expect(page.getByTestId("signup-card")).toBeVisible();
  await expect(page.getByTestId("clinic-name-input")).toBeVisible();
  await expect(page.getByTestId("email-input")).toBeVisible();
  await expect(page.getByTestId("phone-input")).toBeVisible();
  await expect(page.getByTestId("password-input")).toBeVisible();
  await expect(page.getByTestId("confirm-password-input")).toBeVisible();
  await expect(page.getByTestId("signup-submit-button")).toBeVisible();
  await expect(page.getByTestId("signin-link")).toBeVisible();
});

// ─────────────────────────────────────────────────────────────────────────────
// SIGNUP-02 : Successful registration → redirect to dashboard + toast
// ─────────────────────────────────────────────────────────────────────────────
test("SIGNUP-02 successful signup redirects to dashboard with welcome toast", async ({
  page,
}) => {
  await gotoSignup(page);

  const uniqueEmail = `new-clinic-${Date.now()}@test.ae`;

  await fillForm(page, {
    clinicName: "Al Najah Veterinary Clinic",
    email: uniqueEmail,
    phone: "+971 50 999 8888",
    password: "Secure123!",
    confirmPassword: "Secure123!",
  });

  await page.getByTestId("signup-submit-button").click();

  // Should redirect to dashboard
  await expect(page).toHaveURL(/\/(en|ar)\/dashboard/, { timeout: 15000 });

  // Toast should appear with welcome message
  const toast = page.locator('[data-sonner-toast]').first();
  await expect(toast).toBeVisible({ timeout: 10000 });
  await expect(toast).toContainText("Welcome");
});

// ─────────────────────────────────────────────────────────────────────────────
// SIGNUP-03 : Email already taken → inline error
// ─────────────────────────────────────────────────────────────────────────────
test("SIGNUP-03 email already taken shows error message", async ({ page }) => {
  await gotoSignup(page);

  // Use an email that already exists in the MSW mock (one of the MOCK_USERS)
  await fillForm(page, {
    clinicName: "Test Clinic",
    email: "dr.sarah@desertpaws.ae",
    phone: "+971 50 111 2222",
    password: "Secure123!",
    confirmPassword: "Secure123!",
  });

  await page.getByTestId("signup-submit-button").click();

  await expect(page.getByTestId("server-error")).toBeVisible({ timeout: 10000 });
  await expect(page.getByTestId("server-error")).toContainText(
    "An account with this email already exists"
  );
  // Should stay on signup page
  await expect(page).toHaveURL(/\/signup/);
});

// ─────────────────────────────────────────────────────────────────────────────
// SIGNUP-04 : Password strength validation
// ─────────────────────────────────────────────────────────────────────────────
test("SIGNUP-04 password strength hints appear as user types", async ({
  page,
}) => {
  await gotoSignup(page);

  const passwordInput = page.getByTestId("password-input");

  // Type a weak password — hints should appear
  await passwordInput.fill("abc");

  await expect(page.getByTestId("password-strength")).toBeVisible();
  await expect(page.getByTestId("strength-min")).toBeVisible();
  await expect(page.getByTestId("strength-upper")).toBeVisible();
  await expect(page.getByTestId("strength-number")).toBeVisible();

  // Complete the password — all hints should turn green (class text-emerald-600)
  await passwordInput.fill("Secure123");

  await expect(page.getByTestId("strength-min")).toHaveClass(/text-emerald-600/);
  await expect(page.getByTestId("strength-upper")).toHaveClass(/text-emerald-600/);
  await expect(page.getByTestId("strength-number")).toHaveClass(/text-emerald-600/);
});

// ─────────────────────────────────────────────────────────────────────────────
// SIGNUP-05 : Password mismatch shows error
// ─────────────────────────────────────────────────────────────────────────────
test("SIGNUP-05 mismatched passwords show validation error", async ({
  page,
}) => {
  await gotoSignup(page);

  await fillForm(page, {
    clinicName: "Test Clinic",
    email: `mismatch-${Date.now()}@test.ae`,
    phone: "+971 50 111 2222",
    password: "Secure123!",
    confirmPassword: "Different456!",
  });

  await page.getByTestId("signup-submit-button").click();

  await expect(page.getByTestId("confirm-password-error")).toBeVisible({
    timeout: 5000,
  });
  await expect(page.getByTestId("confirm-password-error")).toContainText(
    "Passwords do not match"
  );
  await expect(page).toHaveURL(/\/signup/);
});

// ─────────────────────────────────────────────────────────────────────────────
// SIGNUP-06 : Empty form shows validation errors (client-side, no API call)
// ─────────────────────────────────────────────────────────────────────────────
test("SIGNUP-06 empty form submission shows validation without API call", async ({
  page,
}) => {
  await gotoSignup(page);

  const apiRequests: string[] = [];
  page.on("request", (req) => {
    if (req.url().includes("/api/v1/clinics/register")) {
      apiRequests.push(req.url());
    }
  });

  await page.getByTestId("signup-submit-button").click();

  // Validation errors should appear
  await expect(page.getByTestId("clinic-name-error")).toBeVisible({
    timeout: 5000,
  });
  await expect(page.getByTestId("email-error")).toBeVisible({ timeout: 5000 });

  // No API call should have been made
  expect(apiRequests).toHaveLength(0);
  await expect(page).toHaveURL(/\/signup/);
});

// ─────────────────────────────────────────────────────────────────────────────
// SIGNUP-07 : Landing page "Start Free Trial" CTAs point to /signup
// ─────────────────────────────────────────────────────────────────────────────
test("SIGNUP-07 landing page hero CTA links to signup page", async ({
  page,
}) => {
  await page.goto("/en");
  await page.waitForLoadState("networkidle");

  const heroCta = page.getByTestId("hero-cta-start-trial");
  await expect(heroCta).toBeVisible();

  // Check the href points to /signup
  const href = await heroCta.getAttribute("href");
  expect(href).toContain("/signup");
});

// ─────────────────────────────────────────────────────────────────────────────
// SIGNUP-08 : Nav "Start Free Trial" CTA links to /signup
// ─────────────────────────────────────────────────────────────────────────────
test("SIGNUP-08 landing page nav CTA links to signup page", async ({
  page,
}) => {
  await page.goto("/en");
  await page.waitForLoadState("networkidle");

  const navCta = page.getByTestId("nav-cta-start-trial");
  await expect(navCta).toBeVisible();

  const href = await navCta.getAttribute("href");
  expect(href).toContain("/signup");
});

// ─────────────────────────────────────────────────────────────────────────────
// SIGNUP-09 : Signup page renders in Arabic with RTL layout
// ─────────────────────────────────────────────────────────────────────────────
test("SIGNUP-09 Arabic signup page renders RTL", async ({ page }) => {
  await page.goto("/ar/signup");
  await page.waitForLoadState("networkidle");

  // HTML should have RTL direction
  const html = page.locator("html");
  await expect(html).toHaveAttribute("dir", "rtl");

  // Form fields should be visible
  await expect(page.getByTestId("signup-card")).toBeVisible();
  await expect(page.getByTestId("clinic-name-input")).toBeVisible();

  // Submit button should contain Arabic text
  await expect(page.getByTestId("signup-submit-button")).toContainText(
    "إنشاء عيادتي"
  );
});

// ─────────────────────────────────────────────────────────────────────────────
// SIGNUP-10 : Sign In link on signup page navigates to login
// ─────────────────────────────────────────────────────────────────────────────
test("SIGNUP-10 sign in link navigates to login page", async ({ page }) => {
  await gotoSignup(page);

  const signinLink = page.getByTestId("signin-link");
  await expect(signinLink).toBeVisible();

  const href = await signinLink.getAttribute("href");
  expect(href).toContain("/login");
});
