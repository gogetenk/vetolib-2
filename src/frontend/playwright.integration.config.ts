/**
 * Playwright config for integration tests against the real ASP.NET Core backend.
 *
 * Prerequisites:
 *   1. Start the backend: cd src/backend && dotnet run --project Vetolib.Api
 *   2. Run: npm run test:e2e:integration
 *
 * Seed credentials (from DbInitializer.cs):
 *   admin@desertpaws.ae / Admin123!
 *   dr.sarah@desertpaws.ae / Vet12345!
 */
import { defineConfig, devices } from "@playwright/test";

export default defineConfig({
  testDir: "./e2e",
  // Only run tests tagged @integration — avoids running MSW-only tests
  grep: /@integration/,
  fullyParallel: false, // Integration tests share DB state — run sequentially
  forbidOnly: !!process.env.CI,
  retries: 0,
  workers: 1,
  reporter: [["html", { outputFolder: "playwright-report-integration" }]],
  use: {
    baseURL: process.env.BASE_URL || "http://localhost:6200",
    video: "retain-on-failure",
    screenshot: "only-on-failure",
    trace: "on-first-retry",
    // No service workers needed — MSW is disabled in wire mode
    serviceWorkers: "allow",
    // Pass NEXT_PUBLIC_API_URL so the browser's fetch goes directly to backend
    extraHTTPHeaders: {},
  },
  projects: [
    {
      name: "chromium",
      use: { ...devices["Desktop Chrome"] },
    },
  ],
  webServer: {
    // Launch Next.js with NEXT_PUBLIC_API_URL set — this disables MSW.
    // Uses cross-env-compatible syntax; on Windows use: set NEXT_PUBLIC_API_URL=... &&
    command: "npm run dev:wire -- --port 6200",
    url: "http://localhost:6200",
    reuseExistingServer: true,
    timeout: 120000,
    env: {
      NEXT_PUBLIC_API_URL: "http://localhost:5295",
      BACKEND_URL: "http://localhost:5295",
    },
  },
  // Global setup verifies the backend is up before running tests
  globalSetup: "./e2e/integration/global-setup.ts",
});
