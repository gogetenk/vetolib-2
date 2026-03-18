import { test } from "@playwright/test";
import path from "path";

const screenshotDir = path.resolve(__dirname);
const PORTAL = "/en/portal/desert-paws";
const TOKEN = "valid-magic-token-001";

test.describe("Portal & Booking zone screenshots", () => {
  test.beforeEach(async ({ page }) => {
    // Set the magic-link token by visiting the portal with ?token=
    await page.goto(`${PORTAL}?token=${TOKEN}`);
    await page.waitForLoadState("networkidle");
  });

  test("01 - Portal landing", async ({ page }) => {
    await page.goto(PORTAL);
    await page.waitForLoadState("networkidle");
    await page.screenshot({
      path: path.join(screenshotDir, "01-portal-landing.png"),
      fullPage: true,
    });
  });

  test("02 - Portal landing mobile", async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 812 });
    await page.goto(PORTAL);
    await page.waitForLoadState("networkidle");
    await page.screenshot({
      path: path.join(screenshotDir, "02-portal-landing-mobile.png"),
      fullPage: true,
    });
  });

  test("03 - Portal new message", async ({ page }) => {
    await page.goto(`${PORTAL}/new`);
    await page.waitForLoadState("networkidle");
    await page.screenshot({
      path: path.join(screenshotDir, "03-portal-new-message.png"),
      fullPage: true,
    });
  });

  test("04 - Portal consent", async ({ page }) => {
    await page.goto(`${PORTAL}/consent`);
    await page.waitForLoadState("networkidle");
    await page.screenshot({
      path: path.join(screenshotDir, "04-portal-consent.png"),
      fullPage: true,
    });
  });

  test("05 - Booking landing", async ({ page }) => {
    await page.goto(`${PORTAL}/book`);
    await page.waitForLoadState("networkidle");
    await page.screenshot({
      path: path.join(screenshotDir, "05-booking-landing.png"),
      fullPage: true,
    });
  });

  test("06 - Booking appointments", async ({ page }) => {
    await page.goto(`${PORTAL}/book/appointments`);
    await page.waitForLoadState("networkidle");
    await page.screenshot({
      path: path.join(screenshotDir, "06-booking-appointments.png"),
      fullPage: true,
    });
  });

  test("07 - Portal export", async ({ page }) => {
    await page.goto(`${PORTAL}/export`);
    await page.waitForLoadState("networkidle");
    await page.screenshot({
      path: path.join(screenshotDir, "07-portal-export.png"),
      fullPage: true,
    });
  });

  test("08 - Portal landing Arabic", async ({ page }) => {
    // Set token for Arabic locale too
    await page.goto(`/ar/portal/desert-paws?token=${TOKEN}`);
    await page.waitForLoadState("networkidle");
    await page.screenshot({
      path: path.join(screenshotDir, "08-portal-landing-ar.png"),
      fullPage: true,
    });
  });

  test("09 - Booking Arabic", async ({ page }) => {
    // Set token for Arabic locale too
    await page.goto(`/ar/portal/desert-paws?token=${TOKEN}`);
    await page.waitForLoadState("networkidle");
    await page.goto("/ar/portal/desert-paws/book");
    await page.waitForLoadState("networkidle");
    await page.screenshot({
      path: path.join(screenshotDir, "09-booking-ar.png"),
      fullPage: true,
    });
  });
});
