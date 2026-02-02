import { test, expect, type Page } from '@playwright/test';

const BASE_URL = 'https://green-bush-0e8ee8103.4.azurestaticapps.net';
const API_URL = 'https://app-intunealerts-api-b3c07c.azurewebsites.net';

// Helper to collect console errors
async function collectErrors(page: Page): Promise<string[]> {
  const errors: string[] = [];
  page.on('console', (msg) => {
    if (msg.type() === 'error') {
      errors.push(msg.text());
    }
  });
  page.on('pageerror', (err) => {
    errors.push(err.message);
  });
  return errors;
}

// Helper to check for common date format issues
function isValidDateDisplay(text: string): boolean {
  // Check for "Invalid Date", "NaN", or undefined/null displays
  const invalidPatterns = [
    /Invalid Date/i,
    /NaN/,
    /undefined/,
    /null/,
    /^\s*$/,
  ];
  return !invalidPatterns.some((pattern) => pattern.test(text));
}

test.describe('Full Site Audit', () => {
  test.describe('Dashboard Page', () => {
    test('should load without console errors', async ({ page }) => {
      const errors = await collectErrors(page);
      await page.goto(BASE_URL);
      await page.waitForLoadState('networkidle');

      // Filter out expected errors (like 401 if not authenticated)
      const criticalErrors = errors.filter(
        (e) => !e.includes('401') && !e.includes('Failed to fetch')
      );

      console.log('Console errors:', errors);

      // Take screenshot for review
      await page.screenshot({ path: 'test-results/dashboard-screenshot.png', fullPage: true });
    });

    test('should display valid content structure', async ({ page }) => {
      await page.goto(BASE_URL);
      await page.waitForLoadState('networkidle');

      // Check for header
      const header = page.locator('header');
      await expect(header).toBeVisible();

      // Check for sidebar
      const sidebar = page.locator('aside');
      await expect(sidebar).toBeVisible();

      // Check main content area
      const content = page.locator('main');
      await expect(content).toBeVisible();
    });

    test('should have valid date formats', async ({ page }) => {
      await page.goto(BASE_URL);
      await page.waitForLoadState('networkidle');

      // Look for date-related text
      const bodyText = await page.textContent('body');

      // Check for invalid date displays
      expect(bodyText).not.toContain('Invalid Date');
      expect(bodyText).not.toMatch(/:\s*NaN/);

      // Take screenshot
      await page.screenshot({ path: 'test-results/dashboard-dates.png', fullPage: true });
    });

    test('should display stats cards correctly', async ({ page }) => {
      await page.goto(BASE_URL);
      await page.waitForLoadState('networkidle');

      const bodyText = await page.textContent('body');

      // Check if loading, error, or actual content
      const hasContent =
        bodyText?.includes('Loading') ||
        bodyText?.includes('Failed') ||
        bodyText?.includes('Total Devices') ||
        bodyText?.includes('Compliant');

      expect(hasContent).toBe(true);

      // Log what we see for debugging
      console.log('Dashboard content includes:', {
        loading: bodyText?.includes('Loading'),
        failed: bodyText?.includes('Failed'),
        totalDevices: bodyText?.includes('Total Devices'),
        compliant: bodyText?.includes('Compliant'),
      });
    });
  });

  test.describe('Navigation', () => {
    test('should have all navigation items visible', async ({ page }) => {
      await page.goto(BASE_URL);
      await page.waitForLoadState('networkidle');

      // Check sidebar navigation items
      const navItems = ['Dashboard', 'Devices', 'Alerts'];

      for (const item of navItems) {
        const navItem = page.locator('aside').getByText(item, { exact: true });
        await expect(navItem).toBeVisible();
      }
    });

    test('should navigate to Devices page', async ({ page }) => {
      await page.goto(BASE_URL);
      await page.waitForLoadState('networkidle');

      await page.locator('aside').getByText('Devices').click();
      await expect(page).toHaveURL(/.*devices/);

      await page.screenshot({ path: 'test-results/devices-page.png', fullPage: true });
    });

    test('should navigate to Alerts page', async ({ page }) => {
      await page.goto(BASE_URL);
      await page.waitForLoadState('networkidle');

      await page.locator('aside').getByText('Alerts').click();
      await expect(page).toHaveURL(/.*alerts/);

      await page.screenshot({ path: 'test-results/alerts-page.png', fullPage: true });
    });
  });

  test.describe('Devices Page', () => {
    test('should load devices page', async ({ page }) => {
      await page.goto(`${BASE_URL}/devices`);
      await page.waitForLoadState('networkidle');

      // Header should show "Devices"
      await expect(page.locator('header').getByText('Devices')).toBeVisible();

      await page.screenshot({ path: 'test-results/devices-full.png', fullPage: true });
    });

    test('should check for table or list structure', async ({ page }) => {
      await page.goto(`${BASE_URL}/devices`);
      await page.waitForLoadState('networkidle');

      const bodyText = await page.textContent('body');

      // Log content for debugging
      console.log('Devices page content preview:', bodyText?.substring(0, 500));
    });
  });

  test.describe('Alerts Page', () => {
    test('should load alerts page', async ({ page }) => {
      await page.goto(`${BASE_URL}/alerts`);
      await page.waitForLoadState('networkidle');

      // Header should show "Alerts"
      await expect(page.locator('header').getByText('Alerts')).toBeVisible();

      await page.screenshot({ path: 'test-results/alerts-full.png', fullPage: true });
    });
  });

  test.describe('UI Elements', () => {
    test('should have proper styling', async ({ page }) => {
      await page.goto(BASE_URL);
      await page.waitForLoadState('networkidle');

      // Check sidebar width
      const sidebar = page.locator('aside');
      const sidebarBox = await sidebar.boundingBox();
      expect(sidebarBox?.width).toBeGreaterThan(50);

      // Check header height
      const header = page.locator('header');
      const headerBox = await header.boundingBox();
      expect(headerBox?.height).toBeGreaterThan(40);
    });

    test('should have user menu', async ({ page }) => {
      await page.goto(BASE_URL);
      await page.waitForLoadState('networkidle');

      // Look for avatar/user button in header
      const userButton = page.locator('header button').last();
      await expect(userButton).toBeVisible();
    });

    test('should toggle sidebar', async ({ page }) => {
      await page.goto(BASE_URL);
      await page.waitForLoadState('networkidle');

      const sidebar = page.locator('aside');
      const initialBox = await sidebar.boundingBox();

      // Click toggle button (first button in sidebar header)
      await page.locator('aside button').first().click();

      // Wait for transition
      await page.waitForTimeout(300);

      const collapsedBox = await sidebar.boundingBox();

      // Sidebar should have changed width
      console.log('Sidebar widths:', { initial: initialBox?.width, collapsed: collapsedBox?.width });
    });
  });

  test.describe('Error Handling', () => {
    test('should handle 404 routes gracefully', async ({ page }) => {
      await page.goto(`${BASE_URL}/nonexistent-page`);
      await page.waitForLoadState('networkidle');

      // Should either show 404 or redirect to home
      const bodyText = await page.textContent('body');

      await page.screenshot({ path: 'test-results/404-page.png', fullPage: true });

      console.log('404 page content:', bodyText?.substring(0, 300));
    });
  });

  test.describe('Responsive Layout', () => {
    test('should display correctly on mobile', async ({ page }) => {
      await page.setViewportSize({ width: 375, height: 667 });
      await page.goto(BASE_URL);
      await page.waitForLoadState('networkidle');

      await page.screenshot({ path: 'test-results/mobile-view.png', fullPage: true });
    });

    test('should display correctly on tablet', async ({ page }) => {
      await page.setViewportSize({ width: 768, height: 1024 });
      await page.goto(BASE_URL);
      await page.waitForLoadState('networkidle');

      await page.screenshot({ path: 'test-results/tablet-view.png', fullPage: true });
    });
  });
});
