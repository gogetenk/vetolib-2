import { test, expect, Page } from "@playwright/test"

// Fake JWT tokens for testing

function makeToken(payload: object): string {
  const header = btoa(JSON.stringify({ alg: "HS256", typ: "JWT" })).replace(/=/g, "")
  const body = btoa(
    JSON.stringify({
      ...payload,
      exp: Math.floor(Date.now() / 1000) + 3600,
      iat: Math.floor(Date.now() / 1000),
    })
  ).replace(/=/g, "")
  return `${header}.${body}.mock-signature`
}

const ADMIN_TOKEN = makeToken({
  sub: "admin@desertpaws.ae",
  name: "Omar Al-Rashid",
  role: "ADMIN",
  clinicId: "clinic-001",
  clinicName: "Desert Paws Clinic",
})

const VET_TOKEN = makeToken({
  sub: "dr.sarah@desertpaws.ae",
  name: "Dr. Sarah Johnson",
  role: "VET",
  clinicId: "clinic-001",
  clinicName: "Desert Paws Clinic",
})

const RECEPTIONIST_TOKEN = makeToken({
  sub: "reception@desertpaws.ae",
  name: "Amira Hassan",
  role: "RECEPTIONIST",
  clinicId: "clinic-001",
  clinicName: "Desert Paws Clinic",
})

async function loginAs(page: Page, token: string) {
  await page.context().addCookies([
    {
      name: "access_token",
      value: token,
      domain: "localhost",
      path: "/",
    },
  ])
  await page.goto("/appointments")
  await page.evaluate((t) => {
    localStorage.setItem("access_token", t)
  }, token)
  await page.reload()
  await page.waitForLoadState("networkidle")
}

async function gotoTeamPage(page: Page) {
  await page.goto("/settings/team")
  await page.waitForLoadState("networkidle")
}

test.describe("Team management — ADMIN view", () => {
  test.beforeEach(async ({ page }) => {
    await loginAs(page, ADMIN_TOKEN)
  })

  test("shows team page with member list", async ({ page }) => {
    await gotoTeamPage(page)

    await expect(page.getByTestId("team-page")).toBeVisible()
    await expect(page.getByTestId("team-table")).toBeVisible()

    // Should show the 3 mock users
    await expect(page.getByTestId("user-row-u-001")).toBeVisible()
    await expect(page.getByTestId("user-row-u-002")).toBeVisible()
    await expect(page.getByTestId("user-row-u-003")).toBeVisible()
  })

  test("role badges are displayed with correct labels", async ({ page }) => {
    await gotoTeamPage(page)

    // u-001 is VET, u-002 is RECEPTIONIST, u-003 is ADMIN
    await expect(page.getByTestId("user-role-badge-u-001")).toContainText("VET")
    await expect(page.getByTestId("user-role-badge-u-002")).toContainText("RECEPTIONIST")
    await expect(page.getByTestId("user-role-badge-u-003")).toContainText("ADMIN")
  })

  test("user status badges are shown", async ({ page }) => {
    await gotoTeamPage(page)

    await expect(page.getByTestId("user-status-u-001")).toContainText("Active")
  })

  test("ADMIN sees Invite Member button", async ({ page }) => {
    await gotoTeamPage(page)
    await expect(page.getByTestId("invite-member-btn")).toBeVisible()
  })

  test("ADMIN sees Team link in sidebar", async ({ page }) => {
    await gotoTeamPage(page)
    await expect(page.getByTestId("nav-team")).toBeVisible()
  })

  test("invite member dialog opens and shows form", async ({ page }) => {
    await gotoTeamPage(page)

    await page.getByTestId("invite-member-btn").click()
    await expect(page.getByTestId("invite-user-dialog")).toBeVisible()
    await expect(page.getByTestId("invite-email-input")).toBeVisible()
    await expect(page.getByTestId("invite-fullname-input")).toBeVisible()
    await expect(page.getByTestId("invite-role-select")).toBeVisible()
  })

  test("invite member successfully shows temporary password", async ({ page }) => {
    await gotoTeamPage(page)

    await page.getByTestId("invite-member-btn").click()
    await page.getByTestId("invite-email-input").fill("fatima@desertpaws.ae")
    await page.getByTestId("invite-fullname-input").fill("Dr. Fatima Al-Zaabi")

    // Open the role select and choose VET
    await page.getByTestId("invite-role-select").click()
    await page.getByTestId("invite-role-vet").click()

    await page.getByTestId("invite-submit-btn").click()

    // Should show the temporary password alert
    await expect(page.getByTestId("temp-password-alert")).toBeVisible()
    await expect(page.getByTestId("temp-password-value")).toBeVisible()

    // Copy button should be present
    await expect(page.getByTestId("copy-password-btn")).toBeVisible()
  })

  test("invite dialog validates required fields", async ({ page }) => {
    await gotoTeamPage(page)

    await page.getByTestId("invite-member-btn").click()
    // Submit without filling anything
    await page.getByTestId("invite-submit-btn").click()

    await expect(page.getByTestId("invite-email-error")).toBeVisible()
    await expect(page.getByTestId("invite-fullname-error")).toBeVisible()
  })

  test("change role dialog opens for a user", async ({ page }) => {
    await gotoTeamPage(page)

    await page.getByTestId("change-role-btn-u-001").click()
    await expect(page.getByTestId("change-role-dialog")).toBeVisible()
    await expect(page.getByTestId("change-role-select")).toBeVisible()
  })

  test("change role updates the role badge", async ({ page }) => {
    await gotoTeamPage(page)

    // u-001 is VET — change to ASSISTANT
    await page.getByTestId("change-role-btn-u-001").click()
    await expect(page.getByTestId("change-role-dialog")).toBeVisible()

    // Select ASSISTANT
    await page.getByTestId("change-role-select").click()
    await page.getByTestId("role-option-ASSISTANT").click()

    await page.getByTestId("change-role-confirm-btn").click()

    // Dialog should close
    await expect(page.getByTestId("change-role-dialog")).not.toBeVisible()

    // Role badge should now show ASSISTANT
    await expect(page.getByTestId("user-role-badge-u-001")).toContainText("ASSISTANT")
  })

  test("deactivate button removes user from list", async ({ page }) => {
    await gotoTeamPage(page)

    // Deactivate u-001 (Dr. Sarah)
    await page.getByTestId("deactivate-btn-u-001").click()

    // User row should be removed from the table
    await expect(page.getByTestId("user-row-u-001")).not.toBeVisible()
  })

  test("deactivate button is disabled for the current user", async ({ page }) => {
    await gotoTeamPage(page)

    // The current user is admin@desertpaws.ae (u-003)
    // Their deactivate button should be disabled
    const deactivateBtn = page.getByTestId("deactivate-btn-u-003")
    await expect(deactivateBtn).toBeDisabled()
  })
})

test.describe("Team management — non-ADMIN access control", () => {
  test("VET is redirected away from /settings/team", async ({ page }) => {
    await loginAs(page, VET_TOKEN)
    await page.goto("/settings/team")
    await page.waitForLoadState("networkidle")

    // Should redirect to /appointments
    await expect(page).toHaveURL(/\/(en|ar)\/appointments/)
  })

  test("RECEPTIONIST is redirected away from /settings/team", async ({ page }) => {
    await loginAs(page, RECEPTIONIST_TOKEN)
    await page.goto("/settings/team")
    await page.waitForLoadState("networkidle")

    // Should redirect to /appointments
    await expect(page).toHaveURL(/\/(en|ar)\/appointments/)
  })

  test("VET does not see Team link in sidebar", async ({ page }) => {
    await loginAs(page, VET_TOKEN)
    await page.waitForLoadState("networkidle")
    await expect(page.getByTestId("nav-team")).not.toBeVisible()
  })

  test("RECEPTIONIST does not see Team link in sidebar", async ({ page }) => {
    await loginAs(page, RECEPTIONIST_TOKEN)
    await page.waitForLoadState("networkidle")
    await expect(page.getByTestId("nav-team")).not.toBeVisible()
  })
})
