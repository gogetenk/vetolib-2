import { test, expect } from "@playwright/test";

test.describe("Scaffold smoke test", () => {
  test("login page renders", async ({ page }) => {
    await page.goto("/login");
    await expect(page.getByTestId("login-card")).toBeVisible();
    await expect(page.getByTestId("login-email-input")).toBeVisible();
    await expect(page.getByTestId("login-password-input")).toBeVisible();
    await expect(page.getByTestId("login-submit-btn")).toBeVisible();
  });
});
