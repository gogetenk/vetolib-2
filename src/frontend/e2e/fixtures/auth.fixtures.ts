/**
 * Auth fixtures for the recette test suite.
 *
 * Provides helpers to:
 *  - login via UI (browser flow)
 *  - login via API (headless, returns cookies for context injection)
 *  - get an authenticated API client (using node fetch with cookies)
 *
 * Seed credentials (DbInitializer.cs):
 *   admin@desertpaws.ae  / Admin123!
 *   dr.sarah@desertpaws.ae / Vet12345!
 */
import type { Page, BrowserContext, APIRequestContext } from "@playwright/test";

export const SEED_USERS = {
  admin: {
    email: "admin@desertpaws.ae",
    password: "Admin123!",
    role: "Admin",
  },
  vet: {
    email: "dr.sarah@desertpaws.ae",
    password: "Vet12345!",
    role: "Vet",
  },
} as const;

export const BACKEND_URL =
  process.env.BACKEND_URL ?? "http://localhost:5295";

export type UserKey = keyof typeof SEED_USERS;

/**
 * Login via UI — fills the login form and waits for redirect to appointments.
 * The msw-ready sentinel must be present on the login page (it renders
 * immediately in wire mode with mswReady=true).
 */
export async function loginViaUI(
  page: Page,
  userKey: UserKey = "admin"
): Promise<void> {
  const user = SEED_USERS[userKey];
  await page.goto("/en/login");
  await page.waitForSelector('[data-testid="msw-ready"]', {
    state: "attached",
    timeout: 30000,
  });
  await page.getByTestId("email-input").fill(user.email);
  await page.getByTestId("password-input").fill(user.password);
  await page.getByTestId("signin-button").click();
  await page.waitForURL(/\/(en|ar)\//, { timeout: 20000 });
}

/**
 * Login via API (headless) — calls /api/auth/login directly and returns
 * the Set-Cookie header value so tests can inject it into the browser context.
 */
export async function loginViaAPI(
  request: APIRequestContext,
  userKey: UserKey = "admin"
): Promise<string[]> {
  const user = SEED_USERS[userKey];
  const response = await request.post(`${BACKEND_URL}/api/auth/login`, {
    data: { email: user.email, password: user.password },
  });

  if (!response.ok()) {
    throw new Error(
      `Login API returned ${response.status()} for ${user.email}. ` +
        "Check that the backend is running with correct seed data."
    );
  }

  const headers = response.headers();
  const setCookie = headers["set-cookie"];
  return setCookie ? setCookie.split(",").map((c) => c.trim()) : [];
}

/**
 * Inject authentication cookies into a browser context so tests can skip the
 * UI login flow. Faster for tests that just need an authenticated session.
 */
export async function authenticateContext(
  context: BrowserContext,
  request: APIRequestContext,
  userKey: UserKey = "admin"
): Promise<void> {
  const user = SEED_USERS[userKey];
  // Use Playwright's built-in request context that handles cookies properly
  const response = await request.post(`${BACKEND_URL}/api/auth/login`, {
    data: { email: user.email, password: user.password },
  });

  if (!response.ok()) {
    throw new Error(
      `Authentication failed for ${user.email}: ${response.status()}`
    );
  }
}

/**
 * Wait for the MSW-ready sentinel (present in both MSW and wire modes).
 */
export async function waitForMswReady(page: Page): Promise<void> {
  await page.waitForSelector('[data-testid="msw-ready"]', {
    state: "attached",
    timeout: 30000,
  });
}
