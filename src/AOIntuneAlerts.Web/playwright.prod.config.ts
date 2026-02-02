import { defineConfig, devices } from '@playwright/test';

/**
 * Playwright configuration for testing the production deployment
 * Tests against the live Azure URLs
 */
export default defineConfig({
  testDir: './e2e',
  testMatch: 'full-site-audit.spec.ts',
  fullyParallel: false, // Run sequentially to avoid rate limiting
  forbidOnly: !!process.env.CI,
  retries: 1,
  workers: 1,
  reporter: [['html', { open: 'never' }], ['list']],
  timeout: 60000,

  use: {
    baseURL: 'https://green-bush-0e8ee8103.4.azurestaticapps.net',
    trace: 'on',
    screenshot: 'on',
    video: 'on-first-retry',
  },

  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
  ],

  // No webServer - testing against production
});
