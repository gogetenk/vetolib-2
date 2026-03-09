import { defineConfig, devices } from "@playwright/test";

export default defineConfig({
  testDir: "./e2e",
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,
  reporter: [["html"], ["json", { outputFile: "test-results.json" }]],
  use: {
    baseURL: process.env.BASE_URL || "http://localhost:6100",
    video: "retain-on-failure",
    screenshot: "only-on-failure",
    trace: "on-first-retry",
    // Allow service workers so MSW can intercept API requests in the browser
    serviceWorkers: "allow",
  },
  projects: [
    {
      name: "chromium",
      use: { ...devices["Desktop Chrome"] },
    },
  ],
  webServer: {
    command: "npm run dev:webpack -- --port 6100",
    url: "http://localhost:6100",
    reuseExistingServer: true,
    timeout: 120000,
  },
});
