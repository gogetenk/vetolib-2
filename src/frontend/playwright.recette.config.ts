/**
 * Playwright config for the full recette (acceptance) test suite.
 *
 * Runs against the REAL backend — NOT MSW.
 *
 * Prerequisites:
 *   1. Start the backend: cd src/backend && dotnet run --project Vetolib.Api
 *   2. Run: npm run test:recette
 *
 * Seed credentials (from DbInitializer.cs):
 *   admin@desertpaws.ae  / Admin123!
 *   dr.sarah@desertpaws.ae / Vet12345!
 *
 * The frontend is launched on port 3000 with BACKEND_URL pointing to the API
 * so that Next.js rewrites /api/* to the real backend (MSW disabled).
 */
import { defineConfig, devices } from "@playwright/test";

export default defineConfig({
  testDir: "./e2e/recette",
  fullyParallel: false, // Tests share DB state — run sequentially to avoid flakiness
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 1 : 0,
  workers: 1,
  reporter: [
    ["html", { outputFolder: "playwright-report-recette", open: "never" }],
    ["json", { outputFile: "test-results-recette.json" }],
    ["list"],
  ],
  use: {
    baseURL: process.env.BASE_URL || "http://localhost:3000",
    video: "retain-on-failure",
    screenshot: "only-on-failure",
    trace: "on-first-retry",
    serviceWorkers: "allow",
  },
  projects: [
    {
      name: "chromium",
      use: { ...devices["Desktop Chrome"] },
    },
  ],
  webServer: [
    {
      // Real backend via Vetolib.Api (not the Aspire AppHost for simplicity)
      command:
        "dotnet run --project ../backend/Vetolib.Api/Vetolib.Api.csproj --no-build --urls http://localhost:5295",
      url: "http://localhost:5295/health",
      reuseExistingServer: true,
      timeout: 120000,
    },
    {
      // Next.js with BACKEND_URL set — this disables MSW and proxies /api/* to the real backend
      command: "npm run dev:wire -- --port 3000",
      url: "http://localhost:3000",
      reuseExistingServer: true,
      timeout: 120000,
      env: {
        NEXT_PUBLIC_API_URL: "http://localhost:5295",
        BACKEND_URL: "http://localhost:5295",
        PORT: "3000",
      },
    },
  ],
  globalSetup: "./e2e/recette/global-setup.ts",
});
