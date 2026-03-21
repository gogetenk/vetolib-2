import { defineConfig, globalIgnores } from "eslint/config";
import nextVitals from "eslint-config-next/core-web-vitals";
import nextTs from "eslint-config-next/typescript";

const eslintConfig = defineConfig([
  ...nextVitals,
  ...nextTs,
  // Override default ignores of eslint-config-next.
  globalIgnores([
    // Default ignores of eslint-config-next:
    ".next/**",
    "out/**",
    "build/**",
    "next-env.d.ts",
    // Playwright report artifacts — not source code
    "playwright-report/**",
    "test-results/**",
    // Generated e2e report scripts
    "e2e/reports/**",
    // MSW auto-generated service worker
    "public/mockServiceWorker.js",
  ]),
]);

export default eslintConfig;
