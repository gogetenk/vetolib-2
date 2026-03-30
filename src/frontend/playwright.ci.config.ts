import { defineConfig, devices } from "@playwright/test";

/**
 * CI-specific Playwright config.
 * Uses a production build (`next start`) instead of the dev server for
 * faster, more reliable E2E runs in the nightly workflow.
 */
export default defineConfig({
  testDir: "./e2e",
  fullyParallel: true,
  forbidOnly: true,
  retries: 2,
  workers: 1,
  reporter: [["html"], ["json", { outputFile: "test-results.json" }]],
  use: {
    baseURL: "http://localhost:6100",
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
  webServer: {
    command: "npm run start -- --port 6100",
    url: "http://localhost:6100",
    reuseExistingServer: false,
    timeout: 30000,
  },
});
