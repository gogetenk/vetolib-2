/**
 * Global setup for the recette test suite.
 * Verifies that both the backend and frontend are reachable before running tests.
 */

const BACKEND_URL = process.env.BACKEND_URL ?? "http://localhost:5295";
const FRONTEND_URL = process.env.BASE_URL ?? "http://localhost:3000";
const MAX_RETRIES = 15;
const RETRY_DELAY_MS = 2000;

async function waitForUrl(url: string, label: string): Promise<void> {
  for (let i = 0; i < MAX_RETRIES; i++) {
    try {
      const res = await fetch(url, { signal: AbortSignal.timeout(3000) });
      if (res.status < 500) {
        console.log(`[recette] ${label} is up at ${url}`);
        return;
      }
    } catch {
      // Not ready yet
    }
    console.log(
      `[recette] Waiting for ${label} at ${url}... (${i + 1}/${MAX_RETRIES})`
    );
    await new Promise((r) => setTimeout(r, RETRY_DELAY_MS));
  }
  throw new Error(
    `[recette] ${label} at ${url} did not respond after ${MAX_RETRIES} attempts.`
  );
}

export default async function globalSetup() {
  await waitForUrl(`${BACKEND_URL}/health`, "Backend");
  await waitForUrl(FRONTEND_URL, "Frontend");

  // Verify seed credentials
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
        "[recette] WARNING: Seed credentials admin@desertpaws.ae / Admin123! did not work. " +
          "Check that DbInitializer.SeedAsync ran and Seed:AdminPassword=Admin123! is configured."
      );
    } else {
      console.log("[recette] Seed credentials verified.");
    }
  } catch (e) {
    console.warn("[recette] Could not verify seed credentials:", e);
  }
}
