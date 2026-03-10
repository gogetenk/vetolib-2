import { defineConfig, devices } from "@playwright/test";

export default defineConfig({
  testDir: "./e2e/integration",
  testMatch: "preferences.spec.ts",
  fullyParallel: false,
  forbidOnly: false,
  retries: 0,
  workers: 1,
  reporter: [["list"]],
  use: {
    baseURL: process.env.BASE_URL || "http://localhost:6103",
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
  // No webServer — reuse the running server
});
