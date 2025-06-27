// @ts-check
const { test, expect } = require('@playwright/test');

test.describe('Breadcrumb Navigation', () => {
  test.beforeEach(async ({ page }) => {
    // Set up common test conditions
    await page.goto('/');
  });

  test('should not show breadcrumbs on home page', async ({ page }) => {
    await page.goto('/');
    
    // Home page should not have breadcrumbs
    const breadcrumbContainer = page.locator('.breadcrumb-container');
    await expect(breadcrumbContainer).not.toBeVisible();
  });

  test('should show breadcrumbs on content pages', async ({ page }) => {
    await page.goto('/content/overview.html');
    
    // Should have breadcrumb container
    const breadcrumbContainer = page.locator('.breadcrumb-container');
    await expect(breadcrumbContainer).toBeVisible();
    
    // Should have breadcrumb navigation
    const breadcrumb = page.locator('.breadcrumb');
    await expect(breadcrumb).toBeVisible();
    
    // Should have home link
    const homeLink = page.locator('.breadcrumb-item a[title="Home"]');
    await expect(homeLink).toBeVisible();
    await expect(homeLink).toContainText('');  // Home icon, no text
    
    // Should have current page as active item
    const activeItem = page.locator('.breadcrumb-item.active');
    await expect(activeItem).toBeVisible();
    await expect(activeItem).toContainText('Overview');
  });

  test('should show correct breadcrumb hierarchy for nested pages', async ({ page }) => {
    await page.goto('/content/api/overview.html');
    
    const breadcrumbItems = page.locator('.breadcrumb-item');
    
    // Should have multiple breadcrumb items
    await expect(breadcrumbItems).toHaveCount(4); // Home + Documentation + API Reference + Overview
    
    // Check home link
    const homeItem = breadcrumbItems.nth(0);
    const homeLink = homeItem.locator('a');
    await expect(homeLink).toHaveAttribute('title', 'Home');
    
    // Check intermediate links
    const docItem = breadcrumbItems.nth(1);
    const docLink = docItem.locator('a');
    await expect(docLink).toContainText('Documentation');
    
    const apiItem = breadcrumbItems.nth(2);
    const apiLink = apiItem.locator('a');
    await expect(apiLink).toContainText('API Reference');
    
    // Check active item (no link)
    const activeItem = breadcrumbItems.nth(3);
    await expect(activeItem).toHaveClass(/active/);
    await expect(activeItem).toContainText('Overview');
    await expect(activeItem.locator('a')).not.toBeVisible();
  });

  test('should show breadcrumbs for OpenAPI documentation', async ({ page }) => {
    await page.goto('/content/api/openapi/overview.html');
    
    const breadcrumbItems = page.locator('.breadcrumb-item');
    
    // Should have: Home + Documentation + API Reference + OpenAPI + Overview
    await expect(breadcrumbItems).toHaveCount(5);
    
    // Check OpenAPI section
    const openApiItem = breadcrumbItems.nth(3);
    const openApiLink = openApiItem.locator('a');
    await expect(openApiLink).toContainText('OpenAPI');
    
    // Check active item
    const activeItem = breadcrumbItems.nth(4);
    await expect(activeItem).toHaveClass(/active/);
    await expect(activeItem).toContainText('Overview');
  });

  test('breadcrumb links should be functional', async ({ page }) => {
    await page.goto('/content/api/openapi/overview.html');
    
    // Click on API Reference breadcrumb
    const apiLink = page.locator('.breadcrumb-item a:has-text("API Reference")');
    await apiLink.click();
    
    // Should navigate to API overview page
    await expect(page).toHaveURL(/.*\/content\/api\//);
  });

  test('home breadcrumb should navigate to home page', async ({ page }) => {
    await page.goto('/content/api/overview.html');
    
    // Click home breadcrumb
    const homeLink = page.locator('.breadcrumb-item a[title="Home"]');
    await homeLink.click();
    
    // Should navigate to home page
    await expect(page).toHaveURL(/.*\/index\.html$/);
  });

  test('breadcrumb separators should be present', async ({ page }) => {
    await page.goto('/content/api/overview.html');
    
    // Check that separators are rendered via CSS
    const breadcrumbItems = page.locator('.breadcrumb-item');
    const secondItem = breadcrumbItems.nth(1);
    
    // The separator should be added via CSS ::before pseudo-element
    const separator = await page.evaluate((element) => {
      const style = window.getComputedStyle(element, '::before');
      return style.content;
    }, await secondItem.elementHandle());
    
    expect(separator).toBe('">"');
  });

  test('should have proper accessibility attributes', async ({ page }) => {
    await page.goto('/content/api/overview.html');
    
    // Check breadcrumb navigation has proper ARIA attributes
    const nav = page.locator('nav[aria-label="breadcrumb"]');
    await expect(nav).toBeVisible();
    
    // Check active item has aria-current
    const activeItem = page.locator('.breadcrumb-item.active');
    await expect(activeItem).toHaveAttribute('aria-current', 'page');
    
    // Check home link has screen reader text
    const homeLink = page.locator('.breadcrumb-item a[title="Home"]');
    const srText = homeLink.locator('.sr-only');
    await expect(srText).toContainText('Home');
  });

  test('should work with different documentation sections', async ({ page }) => {
    const testPages = [
      {
        url: '/content/database-integration/overview.html',
        expectedItems: ['Home', 'Documentation', 'Database Integration', 'Overview']
      },
      {
        url: '/content/messaging/overview.html',
        expectedItems: ['Home', 'Documentation', 'Messaging', 'Overview']
      },
      {
        url: '/content/healthchecks/setup.html',
        expectedItems: ['Home', 'Documentation', 'Health Checks', 'Setup']
      }
    ];

    for (const testCase of testPages) {
      await page.goto(testCase.url);
      
      const breadcrumbItems = page.locator('.breadcrumb-item');
      await expect(breadcrumbItems).toHaveCount(testCase.expectedItems.length);
      
      // Check each expected item (skip Home icon for first item)
      for (let i = 1; i < testCase.expectedItems.length; i++) {
        const item = breadcrumbItems.nth(i);
        if (i === testCase.expectedItems.length - 1) {
          // Last item should be active
          await expect(item).toHaveClass(/active/);
          await expect(item).toContainText(testCase.expectedItems[i]);
        } else {
          // Intermediate items should have links
          const link = item.locator('a');
          await expect(link).toContainText(testCase.expectedItems[i]);
        }
      }
    }
  });
});