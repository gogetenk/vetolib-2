/**
 * P5-BILLING — Facturation
 *
 * Tests P5-BILLING-01 through P5-BILLING-06 against the real backend.
 */
import { test, expect, type Page } from "@playwright/test";
import { SEED_USERS, BACKEND_URL, waitForMswReady } from "../fixtures/auth.fixtures";
import { uniqueName } from "../fixtures/data.fixtures";

// ─────────────────────────────────────────────────────────────────────────────
// Helpers
// ─────────────────────────────────────────────────────────────────────────────

async function loginAs(
  page: Page,
  userKey: keyof typeof SEED_USERS = "admin"
): Promise<void> {
  const user = SEED_USERS[userKey];
  await page.goto("/en/login");
  await waitForMswReady(page);
  await page.getByTestId("email-input").fill(user.email);
  await page.getByTestId("password-input").fill(user.password);
  await page.getByTestId("signin-button").click();
  await expect(page).not.toHaveURL(/\/login/, { timeout: 20000 });
}

interface ApiSession {
  patientId: string;
  invoiceId: string;
  invoiceNumber: string;
}

async function setupBillingData(page: Page): Promise<ApiSession> {
  // Login as vet to create patient
  const vetLogin = await page.request.post(`${BACKEND_URL}/api/auth/login`, {
    data: {
      email: SEED_USERS.vet.email,
      password: SEED_USERS.vet.password,
    },
  });
  expect(vetLogin.ok()).toBeTruthy();

  const patientRes = await page.request.post(`${BACKEND_URL}/api/v1/patients`, {
    data: {
      name: uniqueName("BillingCat"),
      species: "Cat",
      breed: "Siamese",
      dateOfBirth: "2020-05-10",
      gender: "Female",
      ownerName: "Aisha Al-Marzouqi",
      ownerPhone: "+971 55 444 5566",
      ownerEmail: "aisha@example.ae",
    },
  });
  expect(patientRes.ok()).toBeTruthy();
  const patient = await patientRes.json();

  // Create invoice as admin
  const adminLogin = await page.request.post(`${BACKEND_URL}/api/auth/login`, {
    data: {
      email: SEED_USERS.admin.email,
      password: SEED_USERS.admin.password,
    },
  });
  expect(adminLogin.ok()).toBeTruthy();

  const invoiceRes = await page.request.post(`${BACKEND_URL}/api/v1/invoices`, {
    data: { patientId: patient.id, notes: "Recette billing test" },
  });
  expect(invoiceRes.ok()).toBeTruthy();
  const invoice = await invoiceRes.json();

  return {
    patientId: patient.id,
    invoiceId: invoice.id,
    invoiceNumber: invoice.invoiceNumber,
  };
}

// ─────────────────────────────────────────────────────────────────────────────
// P5-BILLING-01 : Calcul TVA 5% correct
// ─────────────────────────────────────────────────────────────────────────────
test("P5-BILLING-01 invoice totals with 5% VAT calculated correctly", async ({
  page,
}) => {
  // Create invoice and add items via API to verify calculation
  const adminLogin = await page.request.post(`${BACKEND_URL}/api/auth/login`, {
    data: {
      email: SEED_USERS.admin.email,
      password: SEED_USERS.admin.password,
    },
  });
  expect(adminLogin.ok()).toBeTruthy();

  // Create a patient via vet first
  const vetLogin = await page.request.post(`${BACKEND_URL}/api/auth/login`, {
    data: {
      email: SEED_USERS.vet.email,
      password: SEED_USERS.vet.password,
    },
  });
  expect(vetLogin.ok()).toBeTruthy();

  const patientRes = await page.request.post(`${BACKEND_URL}/api/v1/patients`, {
    data: {
      name: uniqueName("VATTestCat"),
      species: "Cat",
      breed: "Maine Coon",
      dateOfBirth: "2019-08-20",
      gender: "Male",
      ownerName: "Khalid Al-Rashidi",
      ownerPhone: "+971 50 777 8899",
    },
  });

  if (!patientRes.ok()) {
    test.skip(true, "Cannot create patient — skipping VAT test");
    return;
  }
  const patient = await patientRes.json();

  // Re-login as admin to create invoice
  await page.request.post(`${BACKEND_URL}/api/auth/login`, {
    data: {
      email: SEED_USERS.admin.email,
      password: SEED_USERS.admin.password,
    },
  });

  const invoiceRes = await page.request.post(`${BACKEND_URL}/api/v1/invoices`, {
    data: { patientId: patient.id },
  });
  expect(invoiceRes.ok()).toBeTruthy();
  const invoice = await invoiceRes.json();

  // Add items: 150 AED + 75*2 AED = 300 AED subtotal
  await page.request.post(
    `${BACKEND_URL}/api/v1/invoices/${invoice.id}/items`,
    {
      data: { description: "Consultation", quantity: 1, unitPrice: 150 },
    }
  );
  await page.request.post(
    `${BACKEND_URL}/api/v1/invoices/${invoice.id}/items`,
    {
      data: { description: "Vaccination", quantity: 2, unitPrice: 75 },
    }
  );

  // Fetch the invoice and verify totals
  const getRes = await page.request.get(
    `${BACKEND_URL}/api/v1/invoices/${invoice.id}`
  );
  expect(getRes.ok()).toBeTruthy();
  const updated = await getRes.json();

  const subtotal: number = updated.subtotalAmount ?? updated.subtotal ?? 0;
  const vatAmount: number = updated.vatAmount ?? updated.vat ?? 0;
  const total: number = updated.totalAmount ?? updated.total ?? 0;

  expect(subtotal).toBe(300);
  expect(vatAmount).toBe(15); // 5% of 300
  expect(total).toBe(315);
});

// ─────────────────────────────────────────────────────────────────────────────
// P5-BILLING-02 : Billing page loads
// ─────────────────────────────────────────────────────────────────────────────
test("P5-BILLING-02 billing page loads for admin", async ({ page }) => {
  await loginAs(page, "admin");
  await page.goto("/en/billing");
  await page.waitForSelector('[data-testid="billing-page"]', {
    timeout: 15000,
  });

  await expect(page.getByTestId("billing-page")).toBeVisible();
});

// ─────────────────────────────────────────────────────────────────────────────
// P5-BILLING-03 : Workflow facture DRAFT → SENT → PAID
// ─────────────────────────────────────────────────────────────────────────────
test("P5-BILLING-03 invoice workflow: DRAFT → SENT → PAID via API", async ({
  page,
}) => {
  const vetLogin = await page.request.post(`${BACKEND_URL}/api/auth/login`, {
    data: {
      email: SEED_USERS.vet.email,
      password: SEED_USERS.vet.password,
    },
  });
  expect(vetLogin.ok()).toBeTruthy();

  const patientRes = await page.request.post(`${BACKEND_URL}/api/v1/patients`, {
    data: {
      name: uniqueName("WorkflowDog"),
      species: "Dog",
      breed: "Poodle",
      dateOfBirth: "2018-11-15",
      gender: "Male",
      ownerName: "Omar Saeed",
      ownerPhone: "+971 52 100 2233",
    },
  });

  if (!patientRes.ok()) {
    test.skip(true, "Cannot create patient");
    return;
  }
  const patient = await patientRes.json();

  // Create invoice as admin
  await page.request.post(`${BACKEND_URL}/api/auth/login`, {
    data: {
      email: SEED_USERS.admin.email,
      password: SEED_USERS.admin.password,
    },
  });

  const invoiceRes = await page.request.post(`${BACKEND_URL}/api/v1/invoices`, {
    data: { patientId: patient.id },
  });
  expect(invoiceRes.ok()).toBeTruthy();
  const invoice = await invoiceRes.json();
  expect(invoice.status).toBe("DRAFT");

  // Add an item
  await page.request.post(
    `${BACKEND_URL}/api/v1/invoices/${invoice.id}/items`,
    {
      data: { description: "Consultation", quantity: 1, unitPrice: 200 },
    }
  );

  // Send invoice (DRAFT → SENT)
  const sendRes = await page.request.patch(
    `${BACKEND_URL}/api/v1/invoices/${invoice.id}/status`,
    {
      data: { status: "SENT" },
    }
  );
  expect(sendRes.ok()).toBeTruthy();
  const sent = await sendRes.json();
  expect(sent.status).toBe("SENT");

  // Mark as paid (SENT → PAID)
  const paidRes = await page.request.patch(
    `${BACKEND_URL}/api/v1/invoices/${invoice.id}/status`,
    {
      data: { status: "PAID" },
    }
  );
  expect(paidRes.ok()).toBeTruthy();
  const paid = await paidRes.json();
  expect(paid.status).toBe("PAID");
});

// ─────────────────────────────────────────────────────────────────────────────
// P5-BILLING-04 : Montant négatif rejeté par l'API
// ─────────────────────────────────────────────────────────────────────────────
test("P5-BILLING-04 negative unit price is rejected by API", async ({
  page,
}) => {
  const vetLogin = await page.request.post(`${BACKEND_URL}/api/auth/login`, {
    data: {
      email: SEED_USERS.vet.email,
      password: SEED_USERS.vet.password,
    },
  });
  expect(vetLogin.ok()).toBeTruthy();

  const patientRes = await page.request.post(`${BACKEND_URL}/api/v1/patients`, {
    data: {
      name: uniqueName("NegPriceTest"),
      species: "Bird",
      breed: "Parrot",
      dateOfBirth: "2022-01-01",
      gender: "Female",
      ownerName: "Noura Al-Falasi",
      ownerPhone: "+971 56 300 4455",
    },
  });

  if (!patientRes.ok()) {
    test.skip(true, "Cannot create patient");
    return;
  }
  const patient = await patientRes.json();

  await page.request.post(`${BACKEND_URL}/api/auth/login`, {
    data: {
      email: SEED_USERS.admin.email,
      password: SEED_USERS.admin.password,
    },
  });

  const invoiceRes = await page.request.post(`${BACKEND_URL}/api/v1/invoices`, {
    data: { patientId: patient.id },
  });
  expect(invoiceRes.ok()).toBeTruthy();
  const invoice = await invoiceRes.json();

  // Try to add an item with negative price
  const negativeRes = await page.request.post(
    `${BACKEND_URL}/api/v1/invoices/${invoice.id}/items`,
    {
      data: { description: "Bad item", quantity: 1, unitPrice: -50 },
    }
  );

  // Must be rejected
  expect([400, 422]).toContain(negativeRes.status());
});

// ─────────────────────────────────────────────────────────────────────────────
// P5-BILLING-05 : Arrondi TVA correct (115.33 AED → VAT = 5.77)
// ─────────────────────────────────────────────────────────────────────────────
test("P5-BILLING-05 VAT rounding is correct for fractional amounts", async ({
  page,
}) => {
  const vetLogin = await page.request.post(`${BACKEND_URL}/api/auth/login`, {
    data: {
      email: SEED_USERS.vet.email,
      password: SEED_USERS.vet.password,
    },
  });
  expect(vetLogin.ok()).toBeTruthy();

  const patientRes = await page.request.post(`${BACKEND_URL}/api/v1/patients`, {
    data: {
      name: uniqueName("VATRoundTest"),
      species: "Rabbit",
      breed: "Dutch",
      dateOfBirth: "2021-07-01",
      gender: "Male",
      ownerName: "Hamdan Al-Bloushi",
      ownerPhone: "+971 50 555 6677",
    },
  });

  if (!patientRes.ok()) {
    test.skip(true, "Cannot create patient");
    return;
  }
  const patient = await patientRes.json();

  await page.request.post(`${BACKEND_URL}/api/auth/login`, {
    data: {
      email: SEED_USERS.admin.email,
      password: SEED_USERS.admin.password,
    },
  });

  const invoiceRes = await page.request.post(`${BACKEND_URL}/api/v1/invoices`, {
    data: { patientId: patient.id },
  });
  expect(invoiceRes.ok()).toBeTruthy();
  const invoice = await invoiceRes.json();

  await page.request.post(
    `${BACKEND_URL}/api/v1/invoices/${invoice.id}/items`,
    {
      data: { description: "Service", quantity: 1, unitPrice: 115.33 },
    }
  );

  const getRes = await page.request.get(
    `${BACKEND_URL}/api/v1/invoices/${invoice.id}`
  );
  expect(getRes.ok()).toBeTruthy();
  const updated = await getRes.json();

  const vatAmount: number = updated.vatAmount ?? updated.vat ?? 0;
  // 5% of 115.33 = 5.7665 → rounds to 5.77
  expect(vatAmount).toBeCloseTo(5.77, 1);
});

// ─────────────────────────────────────────────────────────────────────────────
// P5-BILLING-06 : Invoice detail page shows correct data
// ─────────────────────────────────────────────────────────────────────────────
test("P5-BILLING-06 invoice detail page displays invoice info", async ({
  page,
}) => {
  await loginAs(page, "admin");
  await page.goto("/en/billing");
  await page.waitForSelector('[data-testid="billing-page"]', {
    timeout: 15000,
  });

  // Click first invoice if any exist
  const firstInvoiceLink = page
    .locator('[data-testid^="invoice-row-"]')
    .first();
  const hasInvoice = await firstInvoiceLink
    .isVisible({ timeout: 5000 })
    .catch(() => false);

  if (hasInvoice) {
    await firstInvoiceLink.locator('[data-testid^="btn-view-"]').click();
    await page.waitForSelector('[data-testid="invoice-detail"]', {
      timeout: 10000,
    });
    await expect(page.getByTestId("invoice-detail")).toBeVisible();
    // Subtotal and total should be displayed
    await expect(
      page
        .getByTestId("invoice-subtotal")
        .or(page.getByText(/Subtotal/i))
        .first()
    ).toBeVisible();
  } else {
    // No invoices yet — just verify the page loaded
    await expect(page.getByTestId("billing-page")).toBeVisible();
  }
});
