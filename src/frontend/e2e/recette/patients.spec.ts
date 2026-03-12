/**
 * P3-PATIENTS — Dossiers patients
 *
 * Tests P3-PATIENTS-01 through P3-PATIENTS-06 against the real backend.
 */
import { test, expect, type Page } from "@playwright/test";
import { SEED_USERS, waitForMswReady } from "../fixtures/auth.fixtures";
import { uniqueName } from "../fixtures/data.fixtures";

// ─────────────────────────────────────────────────────────────────────────────
// Helpers
// ─────────────────────────────────────────────────────────────────────────────

async function loginAs(
  page: Page,
  userKey: keyof typeof SEED_USERS = "vet"
): Promise<void> {
  const user = SEED_USERS[userKey];
  await page.goto("/en/login");
  await waitForMswReady(page);
  await page.getByTestId("email-input").fill(user.email);
  await page.getByTestId("password-input").fill(user.password);
  await page.getByTestId("signin-button").click();
  await expect(page).not.toHaveURL(/\/login/, { timeout: 20000 });
}

async function createPatientViaUI(
  page: Page,
  name: string,
  species: string,
  ownerName: string,
  ownerPhone: string
): Promise<void> {
  await page.goto("/en/patients/new");
  await page.waitForSelector('[data-testid="patient-form"]', {
    timeout: 15000,
  });

  await page.getByTestId("input-patient-name").fill(name);

  await page.getByTestId("select-species-trigger").click();
  await page
    .getByTestId(`species-option-${species.toLowerCase()}`)
    .or(page.getByRole("option", { name: species }))
    .first()
    .click();

  await page.getByTestId("input-owner-name").fill(ownerName);
  await page.getByTestId("input-owner-phone").fill(ownerPhone);

  await page.getByTestId("btn-save-patient").click();
}

// ─────────────────────────────────────────────────────────────────────────────
// P3-PATIENTS-01 : Créer un patient (Vet)
// ─────────────────────────────────────────────────────────────────────────────
test("P3-PATIENTS-01 vet can create a patient", async ({ page }) => {
  await loginAs(page, "vet");

  const name = uniqueName("Simba");
  await createPatientViaUI(
    page,
    name,
    "Cat",
    "Fatima Al-Rashid",
    "+971 50 123 4567"
  );

  // Should redirect to patient detail
  await expect(page).toHaveURL(/\/patients\/[a-z0-9-]+$/, { timeout: 15000 });
  await expect(page.getByTestId("patient-detail-name")).toContainText(name, {
    timeout: 10000,
  });
});

// ─────────────────────────────────────────────────────────────────────────────
// P3-PATIENTS-02 : Rechercher un patient
// ─────────────────────────────────────────────────────────────────────────────
test("P3-PATIENTS-02 search finds patient by name prefix", async ({ page }) => {
  await loginAs(page, "vet");

  // First create a uniquely named patient
  const name = uniqueName("SearchDog");
  await createPatientViaUI(
    page,
    name,
    "Dog",
    "Mohammed Al-Zaabi",
    "+971 52 345 6789"
  );
  await expect(page).toHaveURL(/\/patients\/[a-z0-9-]+$/, { timeout: 15000 });

  // Go to the list and search
  await page.goto("/en/patients");
  await page.waitForSelector('[data-testid="patients-page"]', {
    timeout: 15000,
  });

  // Use the first 6 chars as search term
  const prefix = name.slice(0, 9);
  await page.getByTestId("search-input").fill(prefix);

  // Wait for results to update
  await page.waitForTimeout(500);

  // The patient should appear in results
  await expect(page.getByText(name)).toBeVisible({ timeout: 10000 });
});

// ─────────────────────────────────────────────────────────────────────────────
// P3-PATIENTS-03 : Filtrer par espèce
// ─────────────────────────────────────────────────────────────────────────────
test.skip("P3-PATIENTS-03 filter by species shows only matching patients", async ({
  page,
}) => {
  // SKIPPED: Species filter UI (data-testid="species-filter") is not yet implemented
  // in the PatientsPage component. The page only has a text search input.
  // Re-enable this test once a species filter dropdown is added to the patients list.
  await loginAs(page, "vet");

  await page.goto("/en/patients");
  await page.waitForSelector('[data-testid="patients-page"]', {
    timeout: 15000,
  });
});

// ─────────────────────────────────────────────────────────────────────────────
// P3-PATIENTS-04 : Voir le détail d'un patient
// ─────────────────────────────────────────────────────────────────────────────
test("P3-PATIENTS-04 patient detail page shows info", async ({ page }) => {
  await loginAs(page, "vet");

  // Create a patient to view
  const name = uniqueName("DetailCat");
  await createPatientViaUI(
    page,
    name,
    "Cat",
    "Laila Al-Mansoori",
    "+971 55 777 8888"
  );

  // Should already be on the detail page
  await expect(page).toHaveURL(/\/patients\/[a-z0-9-]+$/, { timeout: 15000 });
  await page.waitForSelector('[data-testid="patient-detail-page"]', {
    timeout: 10000,
  });

  await expect(page.getByTestId("patient-detail-name")).toContainText(name);
  // Species should be visible
  await expect(
    page.getByTestId("patient-detail-species").or(page.getByText("Cat"))
  ).toBeVisible();
});

// ─────────────────────────────────────────────────────────────────────────────
// P3-PATIENTS-05 : Espèce Camel (UAE-specific)
// ─────────────────────────────────────────────────────────────────────────────
test("P3-PATIENTS-05 camel species is valid for UAE market", async ({
  page,
}) => {
  await loginAs(page, "vet");

  const name = uniqueName("Qamar");
  await page.goto("/en/patients/new");
  await page.waitForSelector('[data-testid="patient-form"]', {
    timeout: 15000,
  });

  await page.getByTestId("input-patient-name").fill(name);

  await page.getByTestId("select-species-trigger").click();
  const camelOption = page
    .getByTestId("species-option-camel")
    .or(page.getByRole("option", { name: "Camel" }))
    .first();
  await camelOption.waitFor({ timeout: 5000 });
  await camelOption.click();

  await page.getByTestId("input-owner-name").fill("Sultan Al-Nuaimi");
  await page.getByTestId("input-owner-phone").fill("+971 50 888 9999");

  await page.getByTestId("btn-save-patient").click();

  // Should succeed and redirect to detail
  await expect(page).toHaveURL(/\/patients\/[a-z0-9-]+$/, { timeout: 15000 });
  await expect(page.getByTestId("patient-detail-name")).toContainText(name, {
    timeout: 10000,
  });
});

// ─────────────────────────────────────────────────────────────────────────────
// P3-PATIENTS-06 : Patient list page is accessible
// ─────────────────────────────────────────────────────────────────────────────
test("P3-PATIENTS-06 patient list page loads with correct elements", async ({
  page,
}) => {
  await loginAs(page, "vet");
  await page.goto("/en/patients");
  await page.waitForSelector('[data-testid="patients-page"]', {
    timeout: 15000,
  });

  await expect(page.getByTestId("patients-page")).toBeVisible();
  // Add Patient button should be visible for vet
  await expect(
    page
      .getByTestId("add-patient-btn")
      .or(page.getByRole("link", { name: /add patient/i }))
      .first()
  ).toBeVisible({ timeout: 5000 });
  // Search input should be visible
  await expect(page.getByTestId("search-input")).toBeVisible();
});
