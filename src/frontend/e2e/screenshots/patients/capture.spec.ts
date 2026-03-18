import { test, type Page } from "@playwright/test";
import path from "path";

const screenshotDir = path.resolve(__dirname);

const MAX_PATIENT_ID = "pat-0000-0000-0000-000000000001";

function makeToken(role: string, name: string, email: string): string {
  const payload = {
    sub: email,
    name,
    role,
    clinicId: "clinic-001",
    clinicName: "Desert Paws Clinic",
    exp: Math.floor(Date.now() / 1000) + 3600,
  };
  const header = btoa(JSON.stringify({ alg: "HS256", typ: "JWT" })).replace(
    /=/g,
    ""
  );
  const body = btoa(JSON.stringify(payload)).replace(/=/g, "");
  return `${header}.${body}.fake-signature`;
}

const VET_TOKEN = makeToken(
  "VET",
  "Dr. Sarah Johnson",
  "dr.sarah@desertpaws.ae"
);

async function waitForMSW(page: Page): Promise<void> {
  await page.waitForSelector('[data-testid="msw-ready"]', {
    state: "attached",
    timeout: 30000,
  });
}

async function loginAs(
  page: Page,
  token: string,
  targetPath: string
): Promise<void> {
  await page.context().addCookies([
    {
      name: "access_token",
      value: token,
      domain: "localhost",
      path: "/",
      httpOnly: false,
      secure: false,
    },
  ]);
  await page.addInitScript((t) => {
    localStorage.setItem("access_token", t);
  }, token);
  await page.goto(targetPath);
  await waitForMSW(page);
}

test.describe("Patients & Medical Records zone screenshots", () => {
  // ── Patients List ──────────────────────────────────────────────────────
  test("01 - Patients list", async ({ page }) => {
    await loginAs(page, VET_TOKEN, "/en/patients");
    await page
      .waitForSelector('[data-testid="patients-table"]', { timeout: 15000 })
      .catch(() => {});
    await page.waitForLoadState("networkidle");
    await page.screenshot({
      path: path.join(screenshotDir, "01-patients-list.png"),
      fullPage: true,
    });
  });

  test("02 - Patients list mobile", async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 812 });
    await loginAs(page, VET_TOKEN, "/en/patients");
    await page
      .waitForSelector('[data-testid="patients-table"]', { timeout: 15000 })
      .catch(() => {});
    await page.waitForLoadState("networkidle");
    await page.screenshot({
      path: path.join(screenshotDir, "02-patients-list-mobile.png"),
      fullPage: true,
    });
  });

  test("03 - Patients search", async ({ page }) => {
    await loginAs(page, VET_TOKEN, "/en/patients");
    await page
      .waitForSelector('[data-testid="patients-table"]', { timeout: 15000 })
      .catch(() => {});
    await page.waitForLoadState("networkidle");

    const searchInput = page.getByTestId("search-input");
    await searchInput.fill("Luna");
    await page.waitForTimeout(500);
    await page.waitForLoadState("networkidle");

    await page.screenshot({
      path: path.join(screenshotDir, "03-patients-search.png"),
      fullPage: true,
    });
  });

  // ── Patient Detail ─────────────────────────────────────────────────────
  test("04 - Patient detail", async ({ page }) => {
    await loginAs(page, VET_TOKEN, `/en/patients/${MAX_PATIENT_ID}`);
    await page
      .waitForSelector('[data-testid="patient-detail-page"]', {
        timeout: 20000,
      })
      .catch(() => {});
    await page
      .waitForSelector('[data-testid="patient-detail-name"]', {
        timeout: 20000,
      })
      .catch(() => {});
    await page.waitForTimeout(2000);
    await page.screenshot({
      path: path.join(screenshotDir, "04-patient-detail.png"),
      fullPage: true,
    });
  });

  test("05 - Patient detail mobile", async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 812 });
    await loginAs(page, VET_TOKEN, `/en/patients/${MAX_PATIENT_ID}`);
    await page
      .waitForSelector('[data-testid="patient-detail-page"]', {
        timeout: 20000,
      })
      .catch(() => {});
    await page
      .waitForSelector('[data-testid="patient-detail-name"]', {
        timeout: 20000,
      })
      .catch(() => {});
    await page.waitForTimeout(2000);
    await page.screenshot({
      path: path.join(screenshotDir, "05-patient-detail-mobile.png"),
      fullPage: true,
    });
  });

  // ── New Patient Form ───────────────────────────────────────────────────
  test("06 - New patient form (empty)", async ({ page }) => {
    await loginAs(page, VET_TOKEN, "/en/patients/new");
    await page
      .waitForSelector('[data-testid="new-patient-page"]', { timeout: 15000 })
      .catch(() =>
        page
          .waitForSelector('[data-testid="patient-form"]', { timeout: 10000 })
          .catch(() => {})
      );
    await page.waitForLoadState("networkidle");
    await page.screenshot({
      path: path.join(screenshotDir, "06-patient-new.png"),
      fullPage: true,
    });
  });

  test("07 - New patient form (filled)", async ({ page }) => {
    await loginAs(page, VET_TOKEN, "/en/patients/new");
    await page
      .waitForSelector('[data-testid="patient-form"]', { timeout: 15000 })
      .catch(() => {});
    await page.waitForLoadState("networkidle");

    // Fill fields
    const nameInput = page.getByTestId("input-patient-name");
    if (await nameInput.isVisible().catch(() => false)) {
      await nameInput.fill("Noor");
    }

    const speciesTrigger = page.getByTestId("select-species-trigger");
    if (await speciesTrigger.isVisible().catch(() => false)) {
      await speciesTrigger.click();
      const camelOption = page.getByTestId("species-option-camel");
      if (await camelOption.isVisible().catch(() => false)) {
        await camelOption.click();
      }
    }

    const breedInput = page.getByTestId("input-breed");
    if (await breedInput.isVisible().catch(() => false)) {
      await breedInput.fill("Dromedary");
    }

    const dobInput = page.getByTestId("input-date-of-birth");
    if (await dobInput.isVisible().catch(() => false)) {
      await dobInput.fill("2020-01-15");
    }

    const genderTrigger = page.getByTestId("select-gender-trigger");
    if (await genderTrigger.isVisible().catch(() => false)) {
      await genderTrigger.click();
      const femaleOption = page.getByTestId("gender-option-female");
      if (await femaleOption.isVisible().catch(() => false)) {
        await femaleOption.click();
      }
    }

    const ownerNameInput = page.getByTestId("input-owner-name");
    if (await ownerNameInput.isVisible().catch(() => false)) {
      await ownerNameInput.fill("Hamdan Al-Rashidi");
    }

    const ownerPhoneInput = page.getByTestId("input-owner-phone");
    if (await ownerPhoneInput.isVisible().catch(() => false)) {
      await ownerPhoneInput.fill("+971 56 333 4444");
    }

    await page.waitForTimeout(300);
    await page.screenshot({
      path: path.join(screenshotDir, "07-patient-new-filled.png"),
      fullPage: true,
    });
  });

  // ── Medical Records ────────────────────────────────────────────────────
  test("08 - Medical records", async ({ page }) => {
    await loginAs(page, VET_TOKEN, "/en/medical-records");
    await page.waitForLoadState("networkidle");
    await page.waitForTimeout(2000);
    await page.screenshot({
      path: path.join(screenshotDir, "08-medical-records.png"),
      fullPage: true,
    });
  });

  // ── Arabic ─────────────────────────────────────────────────────────────
  test("09 - Patients list Arabic (RTL)", async ({ page }) => {
    await loginAs(page, VET_TOKEN, "/ar/patients");
    await page
      .waitForSelector('[data-testid="patients-table"]', { timeout: 15000 })
      .catch(() => {});
    await page.waitForLoadState("networkidle");
    await page.screenshot({
      path: path.join(screenshotDir, "09-patients-ar.png"),
      fullPage: true,
    });
  });
});
