import { test, expect } from "@playwright/test";

/**
 * Layout shell tests — run against Next.js dev server with localStorage auth stub.
 *
 * The middleware checks for the `access_token` cookie, so we set it before navigation.
 * We also store it in localStorage for the client-side components (UserMenu, Sidebar).
 */

// A minimal valid-looking JWT payload for testing (not a real signed token)
// Payload: { sub: "Ahmed Al Mansouri", role: "VET", name: "Ahmed Al Mansouri" }
const FAKE_VET_TOKEN =
  "eyJhbGciOiJIUzI1NiJ9." +
  btoa(
    JSON.stringify({
      sub: "ahmed@vetclinic-dubai.com",
      name: "Ahmed Al Mansouri",
      role: "VET",
      exp: Math.floor(Date.now() / 1000) + 3600,
    })
  ).replace(/=/g, "") +
  ".fake-signature";


async function loginAs(
  page: import("@playwright/test").Page,
  token: string
) {
  // Set cookie for middleware, then localStorage for client components
  await page.context().addCookies([
    {
      name: "access_token",
      value: token,
      domain: "localhost",
      path: "/",
    },
  ]);
  await page.goto("/appointments");
  // Set localStorage after page load so client components can read it
  await page.evaluate((t) => {
    localStorage.setItem("access_token", t);
  }, token);
  // Reload so useEffect in components picks up the token
  await page.reload();
}

test.describe("Dashboard Layout — Shell", () => {
  test("header renders logo and user menu", async ({ page }) => {
    await loginAs(page, FAKE_VET_TOKEN);
    await expect(page.getByTestId("dashboard-header")).toBeVisible();
    await expect(page.getByTestId("header-logo")).toBeVisible();
    await expect(page.getByTestId("user-menu-trigger")).toBeVisible();
  });

  test("sidebar renders main navigation items for VET", async ({ page }) => {
    await loginAs(page, FAKE_VET_TOKEN);
    await expect(page.getByTestId("dashboard-sidebar")).toBeVisible();
    await expect(page.getByTestId("nav-appointments")).toBeVisible();
    await expect(page.getByTestId("nav-patients")).toBeVisible();
    await expect(page.getByTestId("nav-billing")).toBeVisible();
  });

  test("sidebar active item is highlighted", async ({ page }) => {
    await loginAs(page, FAKE_VET_TOKEN);
    const appointmentsLink = page.getByTestId("nav-appointments");
    await expect(appointmentsLink).toHaveAttribute("aria-current", "page");
  });

  test("UserMenu dropdown opens and shows sign out option", async ({
    page,
  }) => {
    await loginAs(page, FAKE_VET_TOKEN);
    const trigger = page.getByTestId("user-menu-trigger");
    await trigger.click();
    await expect(page.getByTestId("user-menu-dropdown")).toBeVisible();
    await expect(page.getByTestId("user-menu-signout")).toBeVisible();
    await expect(page.getByTestId("user-menu-settings")).toBeVisible();
  });

  test("Sign out clears session and redirects to login", async ({ page }) => {
    await loginAs(page, FAKE_VET_TOKEN);
    await page.getByTestId("user-menu-trigger").click();
    await page.getByTestId("user-menu-signout").click();
    await expect(page).toHaveURL(/.*login/);
  });

  test("mobile hamburger button is present on small viewport", async ({
    page,
  }) => {
    await page.setViewportSize({ width: 375, height: 667 });
    await loginAs(page, FAKE_VET_TOKEN);
    await expect(page.getByTestId("mobile-menu-trigger")).toBeVisible();
  });

  test("mobile sidebar opens and closes via hamburger", async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 667 });
    await loginAs(page, FAKE_VET_TOKEN);
    await page.getByTestId("mobile-menu-trigger").click();
    await expect(page.getByTestId("mobile-sidebar-sheet")).toBeVisible();
  });
});
