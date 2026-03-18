import { test, Page } from "@playwright/test";
import { loginAsAdmin } from "../../fixtures/messaging";

// Give each test enough time for dev server cold start + MSW init
test.setTimeout(120_000);

const SCREENSHOT_DIR = "e2e/screenshots/messaging";

async function waitForMsw(page: Page) {
  await page.waitForSelector('[data-testid="msw-ready"]', {
    state: "attached",
    timeout: 60000,
  });
}

test.describe("Messaging & Settings Screenshots", () => {
  // ─── Messages ──────────────────────────────────────────────────────────────

  test("01 - Messages inbox", async ({ page }) => {
    await loginAsAdmin(page, "/en/messages");
    try {
      await page.waitForSelector('[data-testid="messages-page"]', {
        timeout: 10000,
      });
    } catch {
      await page.waitForTimeout(3000);
    }
    await page.waitForTimeout(2000);
    await page.screenshot({
      path: `${SCREENSHOT_DIR}/01-messages-inbox.png`,
      fullPage: true,
    });
  });

  test("02 - Messages inbox mobile", async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 812 });
    await loginAsAdmin(page, "/en/messages");
    try {
      await page.waitForSelector('[data-testid="messages-page"]', {
        timeout: 10000,
      });
    } catch {
      await page.waitForTimeout(3000);
    }
    await page.waitForTimeout(2000);
    await page.screenshot({
      path: `${SCREENSHOT_DIR}/02-messages-inbox-mobile.png`,
      fullPage: true,
    });
  });

  test("03 - Message conversation", async ({ page }) => {
    await loginAsAdmin(page, "/en/messages");
    try {
      await page.waitForSelector('[data-testid="messages-page"]', {
        timeout: 10000,
      });
    } catch {
      await page.waitForTimeout(3000);
    }
    await page.waitForTimeout(2000);

    // Click the first conversation in the list
    const firstConversation = page
      .locator(
        '[data-testid^="conversation-item-"], [data-testid="conversation-list-panel"] button, [data-testid="conversation-list-panel"] a, [data-testid="conversation-list-panel"] [role="button"]'
      )
      .first();

    if ((await firstConversation.count()) > 0) {
      await firstConversation.click();
      await page.waitForTimeout(2000);
    }

    await page.screenshot({
      path: `${SCREENSHOT_DIR}/03-message-conversation.png`,
      fullPage: true,
    });
  });

  // ─── Settings - Team ───────────────────────────────────────────────────────

  test("04 - Team list", async ({ page }) => {
    await loginAsAdmin(page, "/en/settings/team");
    try {
      await page.waitForSelector('[data-testid="team-page"]', {
        timeout: 10000,
      });
    } catch {
      await page.waitForTimeout(3000);
    }
    await page.waitForTimeout(2000);
    await page.screenshot({
      path: `${SCREENSHOT_DIR}/04-team-list.png`,
      fullPage: true,
    });
  });

  test("05 - Team invite dialog", async ({ page }) => {
    await loginAsAdmin(page, "/en/settings/team");
    try {
      await page.waitForSelector('[data-testid="team-page"]', {
        timeout: 10000,
      });
    } catch {
      await page.waitForTimeout(3000);
    }
    await page.waitForTimeout(2000);

    // Open invite dialog
    const inviteBtn = page.getByTestId("invite-member-btn");
    if ((await inviteBtn.count()) > 0) {
      await inviteBtn.click();
      await page.waitForTimeout(1000);
      try {
        await page.waitForSelector('[data-testid="invite-user-dialog"]', {
          timeout: 5000,
        });
      } catch {
        // Dialog may already be visible
      }
    }

    await page.screenshot({
      path: `${SCREENSHOT_DIR}/05-team-invite-dialog.png`,
      fullPage: true,
    });
  });

  // ─── Settings - Preferences ────────────────────────────────────────────────

  test("06 - Preferences", async ({ page }) => {
    await loginAsAdmin(page, "/en/settings/preferences");
    try {
      await page.waitForSelector('[data-testid="preferences-page"]', {
        timeout: 10000,
      });
    } catch {
      await page.waitForTimeout(3000);
    }
    await page.waitForTimeout(2000);
    await page.screenshot({
      path: `${SCREENSHOT_DIR}/06-preferences.png`,
      fullPage: true,
    });
  });

  // ─── Settings - Messaging ─────────────────────────────────────────────────

  test("07 - Messaging templates", async ({ page }) => {
    await loginAsAdmin(page, "/en/settings/messaging/templates");
    try {
      await page.waitForSelector('[data-testid="templates-page"]', {
        timeout: 10000,
      });
    } catch {
      await page.waitForTimeout(3000);
    }
    await page.waitForTimeout(2000);
    await page.screenshot({
      path: `${SCREENSHOT_DIR}/07-messaging-templates.png`,
      fullPage: true,
    });
  });

  test("08 - Messaging hours", async ({ page }) => {
    await loginAsAdmin(page, "/en/settings/messaging/hours");
    try {
      await page.waitForSelector('[data-testid="messaging-hours-page"]', {
        timeout: 10000,
      });
    } catch {
      await page.waitForTimeout(3000);
    }
    await page.waitForTimeout(2000);
    await page.screenshot({
      path: `${SCREENSHOT_DIR}/08-messaging-hours.png`,
      fullPage: true,
    });
  });

  test("09 - Messaging stats", async ({ page }) => {
    await loginAsAdmin(page, "/en/settings/messaging/stats");
    try {
      await page.waitForSelector('[data-testid="triage-stats-page"]', {
        timeout: 10000,
      });
    } catch {
      await page.waitForTimeout(3000);
    }
    await page.waitForTimeout(2000);
    await page.screenshot({
      path: `${SCREENSHOT_DIR}/09-messaging-stats.png`,
      fullPage: true,
    });
  });

  // ─── Arabic ────────────────────────────────────────────────────────────────

  test("10 - Messages Arabic", async ({ page }) => {
    await loginAsAdmin(page, "/ar/messages");
    try {
      await page.waitForSelector('[data-testid="messages-page"]', {
        timeout: 10000,
      });
    } catch {
      await page.waitForTimeout(3000);
    }
    await page.waitForTimeout(2000);
    await page.screenshot({
      path: `${SCREENSHOT_DIR}/10-messages-ar.png`,
      fullPage: true,
    });
  });

  test("11 - Team Arabic", async ({ page }) => {
    await loginAsAdmin(page, "/ar/settings/team");
    try {
      await page.waitForSelector('[data-testid="team-page"]', {
        timeout: 10000,
      });
    } catch {
      await page.waitForTimeout(3000);
    }
    await page.waitForTimeout(2000);
    await page.screenshot({
      path: `${SCREENSHOT_DIR}/11-team-ar.png`,
      fullPage: true,
    });
  });
});
