/**
 * Data fixtures for the recette test suite.
 *
 * Provides helpers to create test data via the API so tests are independent.
 * All data uses UAE-realistic values (Arabic names, AED currency, UAE phone numbers).
 */
import type { APIRequestContext } from "@playwright/test";
import { BACKEND_URL } from "./auth.fixtures";

// ─────────────────────────────────────────────────────────────────────────────
// Types mirroring backend DTOs
// ─────────────────────────────────────────────────────────────────────────────

export interface PatientData {
  id: string;
  name: string;
  species: string;
  ownerName: string;
  ownerPhone: string;
}

export interface AppointmentData {
  id: string;
  patientName: string;
  species: string;
  ownerName: string;
  ownerPhone: string;
  vetId: string;
  scheduledAt: string;
  reason: string;
  status: string;
}

export interface InvoiceData {
  id: string;
  invoiceNumber: string;
  patientId: string;
  status: string;
  subtotal: number;
  vatAmount: number;
  total: number;
}

// ─────────────────────────────────────────────────────────────────────────────
// Helpers
// ─────────────────────────────────────────────────────────────────────────────

/**
 * Returns a unique name suffixed with a timestamp to avoid collisions between
 * test runs on a shared database.
 */
export function uniqueName(prefix: string): string {
  return `${prefix}-${Date.now()}`;
}

/**
 * ISO timestamp for "tomorrow at HH:MM" in UTC.
 */
export function tomorrowAt(hour: number, minute = 0): string {
  const d = new Date();
  d.setDate(d.getDate() + 1);
  d.setUTCHours(hour, minute, 0, 0);
  return d.toISOString();
}

/**
 * ISO timestamp for "in N days at HH:MM" in UTC.
 */
export function inDaysAt(days: number, hour: number, minute = 0): string {
  const d = new Date();
  d.setDate(d.getDate() + days);
  d.setUTCHours(hour, minute, 0, 0);
  return d.toISOString();
}

// ─────────────────────────────────────────────────────────────────────────────
// Patient fixtures
// ─────────────────────────────────────────────────────────────────────────────

export async function createPatient(
  request: APIRequestContext,
  overrides: Partial<{
    name: string;
    species: string;
    breed: string;
    ownerName: string;
    ownerPhone: string;
    ownerEmail: string;
  }> = {}
): Promise<PatientData> {
  const data = {
    name: overrides.name ?? uniqueName("Simba"),
    species: overrides.species ?? "Cat",
    breed: overrides.breed ?? "Persian",
    dateOfBirth: "2021-03-15",
    gender: "Male",
    ownerName: overrides.ownerName ?? "Fatima Al-Rashid",
    ownerPhone: overrides.ownerPhone ?? "+971 50 123 4567",
    ownerEmail: overrides.ownerEmail ?? "fatima@example.ae",
  };

  const response = await request.post(`${BACKEND_URL}/api/v1/patients`, {
    data,
  });

  if (!response.ok()) {
    const body = await response.text();
    throw new Error(
      `Failed to create patient: ${response.status()} — ${body}`
    );
  }

  const json = await response.json();
  return {
    id: json.id,
    name: data.name,
    species: data.species,
    ownerName: data.ownerName,
    ownerPhone: data.ownerPhone,
  };
}

// ─────────────────────────────────────────────────────────────────────────────
// Appointment fixtures
// ─────────────────────────────────────────────────────────────────────────────

export async function createAppointment(
  request: APIRequestContext,
  overrides: Partial<{
    patientName: string;
    species: string;
    ownerName: string;
    ownerPhone: string;
    vetId: string;
    scheduledAt: string;
    reason: string;
  }> = {}
): Promise<AppointmentData> {
  const data = {
    patientName: overrides.patientName ?? uniqueName("Max"),
    species: overrides.species ?? "Dog",
    ownerName: overrides.ownerName ?? "Ahmed Al-Mansoori",
    ownerPhone: overrides.ownerPhone ?? "+971 50 999 0001",
    vetId: overrides.vetId ?? "00000000-0000-0000-0002-000000000002", // Dr. Sarah seed ID
    scheduledAt: overrides.scheduledAt ?? tomorrowAt(10),
    reason: overrides.reason ?? "Annual checkup",
  };

  const response = await request.post(`${BACKEND_URL}/api/v1/appointments`, {
    data,
  });

  if (!response.ok()) {
    const body = await response.text();
    throw new Error(
      `Failed to create appointment: ${response.status()} — ${body}`
    );
  }

  const json = await response.json();
  return {
    id: json.id,
    patientName: data.patientName,
    species: data.species,
    ownerName: data.ownerName,
    ownerPhone: data.ownerPhone,
    vetId: data.vetId,
    scheduledAt: data.scheduledAt,
    reason: data.reason,
    status: json.status ?? "SCHEDULED",
  };
}

// ─────────────────────────────────────────────────────────────────────────────
// Invoice fixtures
// ─────────────────────────────────────────────────────────────────────────────

export async function createInvoice(
  request: APIRequestContext,
  patientId: string
): Promise<InvoiceData> {
  const response = await request.post(`${BACKEND_URL}/api/v1/invoices`, {
    data: { patientId, notes: "Recette test invoice" },
  });

  if (!response.ok()) {
    const body = await response.text();
    throw new Error(
      `Failed to create invoice: ${response.status()} — ${body}`
    );
  }

  const json = await response.json();
  return {
    id: json.id,
    invoiceNumber: json.invoiceNumber,
    patientId,
    status: json.status ?? "DRAFT",
    subtotal: json.subtotalAmount ?? 0,
    vatAmount: json.vatAmount ?? 0,
    total: json.totalAmount ?? 0,
  };
}

export async function addInvoiceItem(
  request: APIRequestContext,
  invoiceId: string,
  description: string,
  quantity: number,
  unitPrice: number
): Promise<void> {
  const response = await request.post(
    `${BACKEND_URL}/api/v1/invoices/${invoiceId}/items`,
    {
      data: { description, quantity, unitPrice },
    }
  );

  if (!response.ok()) {
    const body = await response.text();
    throw new Error(
      `Failed to add invoice item: ${response.status()} — ${body}`
    );
  }
}
