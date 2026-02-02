import { defineConfig, devices } from '@playwright/test';

/**
 * Playwright configuration for Intune Compliance Portal
 * Uses dev auth bypass for automated testing
 *
 * To run tests:
 * 1. Run: npm run test:e2e (starts mock API and frontend automatically)
 *
 * Or run with UI: npm run test:e2e:ui
 */
export default defineConfig({
  testDir: './e2e',
  testIgnore: ['**/full-site-audit.spec.ts'], // Ignore production tests
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,
  reporter: [['html', { open: 'never' }], ['list']],
  timeout: 30000,

  use: {
    // Base URL for the frontend (dev server in test mode)
    baseURL: process.env.PLAYWRIGHT_BASE_URL || 'http://localhost:5173',

    // Collect trace on first retry
    trace: 'on-first-retry',

    // Screenshot on failure
    screenshot: 'only-on-failure',
  },

  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
  ],

  // Run mock API and frontend dev server before tests
  webServer: [
    {
      command: 'node mock-api/server.js',
      url: 'http://localhost:5001/health',
      reuseExistingServer: !process.env.CI,
      timeout: 30000,
    },
    {
      command: 'npm run dev:test',
      url: 'http://localhost:5173',
      reuseExistingServer: !process.env.CI,
      timeout: 120000,
      env: {
        VITE_DEV_AUTH_ENABLED: 'true',
        VITE_DEV_AUTH_SECRET: 'dev-test-secret-12345',
        VITE_DEV_AUTH_USER: 'playwright@test.local',
        VITE_DEV_AUTH_ROLE: 'Admin',
        VITE_API_BASE_URL: 'http://localhost:5001',
        VITE_AZURE_AD_CLIENT_ID: '00000000-0000-0000-0000-000000000000',
        VITE_AZURE_AD_TENANT_ID: '00000000-0000-0000-0000-000000000000',
      },
    },
  ],
});
