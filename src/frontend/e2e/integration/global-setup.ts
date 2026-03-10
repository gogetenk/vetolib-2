/**
 * Global setup for integration tests.
 * Verifies that the ASP.NET Core backend is reachable before running tests.
 */
import { chromium } from "@playwright/test";

const BACKEND_URL = process.env.BACKEND_URL ?? "http://localhost:5295";
const MAX_RETRIES = 10;
const RETRY_DELAY_MS = 2000;

async function waitForBackend(): Promise<void> {
  for (let i = 0; i < MAX_RETRIES; i++) {
    try {
      const res = await fetch(`${BACKEND_URL}/health`);
      if (res.ok) {
        console.log(`Backend is up at ${BACKEND_URL}`);
        return;
      }
    } catch {
      // Not ready yet
    }
    console.log(
      `Waiting for backend at ${BACKEND_URL}... (attempt ${i + 1}/${MAX_RETRIES})`
    );
    await new Promise((r) => setTimeout(r, RETRY_DELAY_MS));
  }
  throw new Error(
    `Backend at ${BACKEND_URL} did not respond after ${MAX_RETRIES} attempts. ` +
      `Start the backend with: cd src/backend && dotnet run --project Vetolib.Api`
  );
}

export default async function globalSetup() {
  await waitForBackend();

  // Optionally verify seed credentials work
  try {
    const res = await fetch(`${BACKEND_URL}/api/auth/login`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        email: "admin@desertpaws.ae",
        password: "Admin123!",
      }),
    });
    if (!res.ok) {
      console.warn(
        "Warning: Seed credentials admin@desertpaws.ae / Admin123! did not work. " +
          "Check that DbInitializer.SeedAsync ran successfully."
      );
    } else {
      console.log("Seed credentials verified successfully.");
    }
  } catch (e) {
    console.warn("Warning: Could not verify seed credentials:", e);
  }
}
