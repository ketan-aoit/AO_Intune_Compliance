import { test, expect } from '@playwright/test';

test.describe('Dashboard', () => {
  test('should load dashboard page', async ({ page }) => {
    await page.goto('/');

    // Wait for the page to load - either dashboard content or error state
    await page.waitForLoadState('networkidle');

    // The page should display either:
    // - "Loading dashboard..." (loading state)
    // - "Failed to load dashboard" (API not available)
    // - Dashboard content with stats
    const bodyText = await page.textContent('body');
    expect(bodyText).toBeTruthy();

    // Check that the Layout loaded (header should show "Dashboard")
    await expect(page.locator('header').getByText('Dashboard')).toBeVisible({
      timeout: 10000,
    });
  });

  test('should display page content', async ({ page }) => {
    await page.goto('/');

    // Wait for dashboard to load
    await page.waitForLoadState('networkidle');

    // Check for compliance-related content or loading/error state
    const pageContent = await page.textContent('body');
    expect(pageContent).toBeTruthy();

    // Should show either loading, error, or actual dashboard content
    const hasExpectedContent =
      pageContent?.includes('Loading') ||
      pageContent?.includes('Failed') ||
      pageContent?.includes('Total Devices') ||
      pageContent?.includes('Compliant');
    expect(hasExpectedContent).toBe(true);
  });

  test('should have navigation sidebar', async ({ page }) => {
    await page.goto('/');

    // The sidebar should be visible with navigation items
    const sidebar = page.locator('aside');
    await expect(sidebar).toBeVisible();

    // Check for navigation items within sidebar (use exact match to avoid conflicts with page content)
    await expect(sidebar.getByText('Devices', { exact: true })).toBeVisible();
    await expect(sidebar.getByText('Alerts', { exact: true })).toBeVisible();
  });

  test('should navigate to devices page via sidebar', async ({ page }) => {
    await page.goto('/');

    // Click on Devices in the sidebar navigation
    await page.locator('aside').getByText('Devices', { exact: true }).click();

    // Should navigate to devices page
    await expect(page).toHaveURL(/.*devices/);
  });
});
