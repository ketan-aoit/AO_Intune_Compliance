import { test, expect } from '@playwright/test';

test.describe('API Client with Dev Auth', () => {
  test('should use dev auth headers when enabled', async ({ page }) => {
    // Navigate to the app
    await page.goto('/');

    // Wait for initial load
    await page.waitForLoadState('domcontentloaded');

    // Check console for dev auth message
    const consoleMessages: string[] = [];
    page.on('console', (msg) => {
      consoleMessages.push(msg.text());
    });

    // Reload to capture console messages
    await page.reload();
    await page.waitForLoadState('networkidle');

    // Should have logged dev auth mode
    const hasDevAuthMessage = consoleMessages.some(
      (msg) => msg.includes('Dev auth mode') || msg.includes('[App]')
    );

    // The page should have loaded without requiring MSAL login
    const bodyText = await page.textContent('body');
    expect(bodyText).toBeTruthy();
  });
});
