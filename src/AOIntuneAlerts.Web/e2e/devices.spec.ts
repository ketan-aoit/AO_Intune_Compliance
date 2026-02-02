import { test, expect } from '@playwright/test';

test.describe('Devices Page', () => {
  test('should navigate to devices page', async ({ page }) => {
    await page.goto('/');

    // Click on Devices in the sidebar
    await page.getByText('Devices').click();

    // Should be on devices page
    await expect(page).toHaveURL(/.*devices/);

    // Header should show "Devices"
    await expect(page.locator('header').getByText('Devices')).toBeVisible();
  });

  test('should display devices page content', async ({ page }) => {
    await page.goto('/devices');

    // Wait for page to load
    await page.waitForLoadState('networkidle');

    // Page should have loaded (either with data, loading, or error state)
    const bodyText = await page.textContent('body');
    expect(bodyText).toBeTruthy();

    // Header should show "Devices"
    await expect(page.locator('header').getByText('Devices')).toBeVisible();
  });
});
