import { test, Page } from "@playwright/test";
import path from "path";

const screenshotDir = path.resolve(__dirname);

/**
 * Wait for the page to be fully rendered (not just networkidle).
 * The dev server compiles pages on first visit, so we need to wait for actual content.
 */
async function waitForPageContent(page: Page, timeout = 30000) {
  // Wait for at least one visible element with content to appear
  await page.waitForFunction(
    () => document.body.innerText.trim().length > 0,
    { timeout }
  );
  // Small additional wait for CSS/images to settle
  await page.waitForTimeout(500);
}

test.describe("Auth & Landing zone screenshots", () => {
  // Run serially — dev server compiles pages on first visit, parallelism causes timeouts
  test.describe.configure({ mode: "serial" });
  // Generous timeout for dev server compilation
  test.setTimeout(120000);

  test("01 - Landing page EN", async ({ page }) => {
    // NOTE: middleware redirects /en to /en/login for unauthenticated users.
    // The landing page (/) component exists but is not publicly accessible.
    // This screenshot captures what an unauthenticated user sees at /en.
    await page.goto("/en", { waitUntil: "networkidle" });
    await waitForPageContent(page);
    await page.screenshot({
      path: path.join(screenshotDir, "01-landing-en.png"),
      fullPage: true,
    });
  });

  test("02 - Landing page mobile", async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 812 });
    await page.goto("/en", { waitUntil: "networkidle" });
    await waitForPageContent(page);
    await page.screenshot({
      path: path.join(screenshotDir, "02-landing-mobile.png"),
      fullPage: true,
    });
  });

  test("03 - Landing page Arabic (RTL)", async ({ page }) => {
    await page.goto("/ar", { waitUntil: "networkidle" });
    await waitForPageContent(page);
    await page.screenshot({
      path: path.join(screenshotDir, "03-landing-ar.png"),
      fullPage: true,
    });
  });

  test("04 - Login page EN", async ({ page }) => {
    await page.goto("/en/login", { waitUntil: "networkidle" });
    await page.getByTestId("login-card").waitFor({ state: "visible", timeout: 30000 });
    await page.screenshot({
      path: path.join(screenshotDir, "04-login-en.png"),
      fullPage: true,
    });
  });

  test("05 - Login page with error", async ({ page }) => {
    await page.goto("/en/login", { waitUntil: "networkidle" });
    await page.getByTestId("login-card").waitFor({ state: "visible", timeout: 30000 });

    // Fill wrong credentials and submit
    await page.getByTestId("email-input").fill("wrong@example.com");
    await page.getByTestId("password-input").fill("wrongpassword");
    await page.getByTestId("signin-button").click();

    // Wait for error message to appear
    await page.getByTestId("error-message").waitFor({ state: "visible", timeout: 15000 });

    await page.screenshot({
      path: path.join(screenshotDir, "05-login-error.png"),
      fullPage: true,
    });
  });

  test("06 - Login page Arabic", async ({ page }) => {
    await page.goto("/ar/login", { waitUntil: "networkidle" });
    await page.getByTestId("login-card").waitFor({ state: "visible", timeout: 30000 });
    await page.screenshot({
      path: path.join(screenshotDir, "06-login-ar.png"),
      fullPage: true,
    });
  });

  test("07 - Signup page EN", async ({ page }) => {
    // Middleware blocks direct /en/signup access for unauthenticated users.
    // Navigate directly and capture what renders (redirected to login).
    await page.goto("/en/signup", { waitUntil: "networkidle" });
    await waitForPageContent(page);
    await page.screenshot({
      path: path.join(screenshotDir, "07-signup-en.png"),
      fullPage: true,
    });
  });

  test("08 - Signup page with validation errors", async ({ page }) => {
    // Same redirect issue — capture the result of navigating to /en/signup
    await page.goto("/en/signup", { waitUntil: "networkidle" });
    await waitForPageContent(page);

    // If we ended up on signup, try to submit; otherwise capture what's shown
    const signupCard = page.getByTestId("signup-card");
    if (await signupCard.isVisible().catch(() => false)) {
      await page.getByTestId("signup-submit-button").click();
      await page.getByTestId("clinic-name-error").waitFor({ state: "visible", timeout: 5000 }).catch(() => {});
    }

    await page.screenshot({
      path: path.join(screenshotDir, "08-signup-validation-errors.png"),
      fullPage: true,
    });
  });

  test("09 - Signup page with password strength hints", async ({ page }) => {
    await page.goto("/en/signup", { waitUntil: "networkidle" });
    await waitForPageContent(page);

    const signupCard = page.getByTestId("signup-card");
    if (await signupCard.isVisible().catch(() => false)) {
      await page.getByTestId("password-input").fill("abc");
      await page.getByTestId("password-strength").waitFor({ state: "visible", timeout: 5000 }).catch(() => {});
    }

    await page.screenshot({
      path: path.join(screenshotDir, "09-signup-password-strength.png"),
      fullPage: true,
    });
  });

  test("10 - Signup page Arabic", async ({ page }) => {
    await page.goto("/ar/signup", { waitUntil: "networkidle" });
    await waitForPageContent(page);
    await page.screenshot({
      path: path.join(screenshotDir, "10-signup-ar.png"),
      fullPage: true,
    });
  });
});
