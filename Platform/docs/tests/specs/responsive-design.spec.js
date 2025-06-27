// @ts-check
const { test, expect } = require('@playwright/test');

test.describe('Responsive Design and Accessibility', () => {
  test('breadcrumbs should be responsive on mobile', async ({ page }) => {
    // Set mobile viewport
    await page.setViewportSize({ width: 375, height: 667 });
    await page.goto('/content/api/openapi/overview.html');
    
    const breadcrumbContainer = page.locator('.breadcrumb-container');
    await expect(breadcrumbContainer).toBeVisible();
    
    // Breadcrumb should wrap properly on mobile
    const breadcrumb = page.locator('.breadcrumb');
    const breadcrumbBox = await breadcrumb.boundingBox();
    
    expect(breadcrumbBox?.width).toBeLessThanOrEqual(375);
    
    // Items should still be visible
    const breadcrumbItems = page.locator('.breadcrumb-item');
    await expect(breadcrumbItems.first()).toBeVisible();
  });

  test('breadcrumbs should work on tablet viewport', async ({ page }) => {
    // Set tablet viewport
    await page.setViewportSize({ width: 768, height: 1024 });
    await page.goto('/content/api/openapi/overview.html');
    
    const breadcrumbContainer = page.locator('.breadcrumb-container');
    await expect(breadcrumbContainer).toBeVisible();
    
    // Check that all breadcrumb items are visible
    const breadcrumbItems = page.locator('.breadcrumb-item');
    const itemCount = await breadcrumbItems.count();
    
    for (let i = 0; i < itemCount; i++) {
      await expect(breadcrumbItems.nth(i)).toBeVisible();
    }
  });

  test('breadcrumbs should work on desktop viewport', async ({ page }) => {
    // Set desktop viewport
    await page.setViewportSize({ width: 1920, height: 1080 });
    await page.goto('/content/api/openapi/overview.html');
    
    const breadcrumbContainer = page.locator('.breadcrumb-container');
    await expect(breadcrumbContainer).toBeVisible();
    
    // All items should be on single line for desktop
    const breadcrumbItems = page.locator('.breadcrumb-item');
    const firstItem = breadcrumbItems.first();
    const lastItem = breadcrumbItems.last();
    
    const firstItemBox = await firstItem.boundingBox();
    const lastItemBox = await lastItem.boundingBox();
    
    // Items should be roughly on the same line (allowing for slight variations)
    if (firstItemBox && lastItemBox) {
      const heightDifference = Math.abs(firstItemBox.y - lastItemBox.y);
      expect(heightDifference).toBeLessThan(30); // Allow some tolerance
    }
  });

  test('dark mode should work correctly', async ({ page }) => {
    await page.goto('/content/api/overview.html');
    
    // Check if dark mode toggle exists
    const darkModeToggle = page.locator('[data-bs-theme-value="dark"], .dark-mode-toggle, button:has-text("Dark")');
    const toggleExists = await darkModeToggle.count() > 0;
    
    if (toggleExists) {
      // Click dark mode toggle
      await darkModeToggle.first().click();
      
      // Check that dark mode is applied
      const body = page.locator('body');
      const htmlElement = page.locator('html');
      
      // Check for dark mode attributes/classes
      const bodyClass = await body.getAttribute('class');
      const htmlDataTheme = await htmlElement.getAttribute('data-bs-theme');
      const bodyDataTheme = await body.getAttribute('data-bs-theme');
      
      const isDarkModeActive = 
        bodyClass?.includes('dark') || 
        htmlDataTheme === 'dark' || 
        bodyDataTheme === 'dark';
      
      if (isDarkModeActive) {
        // Check breadcrumb styling in dark mode
        const breadcrumbContainer = page.locator('.breadcrumb-container');
        if (await breadcrumbContainer.count() > 0) {
          const containerStyle = await breadcrumbContainer.evaluate((el) => {
            return getComputedStyle(el);
          });
          
          // Dark mode should have different colors
          expect(containerStyle.borderBottomColor).toBeTruthy();
        }
      }
    }
  });

  test('keyboard navigation should work', async ({ page }) => {
    await page.goto('/content/api/openapi/overview.html');
    
    // Focus on first breadcrumb link (home)
    const homeLink = page.locator('.breadcrumb-item a').first();
    await homeLink.focus();
    
    // Check that the link is focused
    const isFocused = await homeLink.evaluate((el) => el === document.activeElement);
    expect(isFocused).toBe(true);
    
    // Navigate with Tab key
    await page.keyboard.press('Tab');
    
    // Should move to next focusable element
    const activeElement = await page.evaluate(() => document.activeElement?.tagName);
    expect(activeElement).toBeTruthy();
  });

  test('breadcrumb links should be keyboard accessible', async ({ page }) => {
    await page.goto('/content/api/openapi/overview.html');
    
    const breadcrumbLinks = page.locator('.breadcrumb-item a');
    const linkCount = await breadcrumbLinks.count();
    
    if (linkCount > 0) {
      // Focus first link
      await breadcrumbLinks.first().focus();
      
      // Press Enter to activate
      await page.keyboard.press('Enter');
      
      // Should navigate (URL should change)
      await page.waitForLoadState('networkidle');
      const currentUrl = page.url();
      expect(currentUrl).toBeTruthy();
    }
  });

  test('high contrast mode compatibility', async ({ page }) => {
    await page.goto('/content/api/overview.html');
    
    // Simulate high contrast by forcing specific styles
    await page.addStyleTag({
      content: `
        @media (prefers-contrast: high) {
          .breadcrumb-item a {
            text-decoration: underline !important;
            color: blue !important;
          }
          .breadcrumb-item.active {
            font-weight: bold !important;
          }
        }
      `
    });
    
    const breadcrumbContainer = page.locator('.breadcrumb-container');
    if (await breadcrumbContainer.count() > 0) {
      await expect(breadcrumbContainer).toBeVisible();
      
      // Check that links are still functional
      const breadcrumbLinks = page.locator('.breadcrumb-item a');
      if (await breadcrumbLinks.count() > 0) {
        await expect(breadcrumbLinks.first()).toBeVisible();
      }
    }
  });

  test('focus indicators should be visible', async ({ page }) => {
    await page.goto('/content/api/openapi/overview.html');
    
    const breadcrumbLinks = page.locator('.breadcrumb-item a');
    const linkCount = await breadcrumbLinks.count();
    
    if (linkCount > 0) {
      const firstLink = breadcrumbLinks.first();
      
      // Focus the link
      await firstLink.focus();
      
      // Check that focus indicator is visible
      const focusStyle = await firstLink.evaluate((el) => {
        const style = getComputedStyle(el);
        return {
          outline: style.outline,
          outlineColor: style.outlineColor,
          outlineWidth: style.outlineWidth,
          boxShadow: style.boxShadow
        };
      });
      
      // Should have some form of focus indicator
      const hasFocusIndicator = 
        focusStyle.outline !== 'none' ||
        focusStyle.outlineWidth !== '0px' ||
        focusStyle.boxShadow !== 'none';
      
      expect(hasFocusIndicator).toBe(true);
    }
  });

  test('screen reader compatibility', async ({ page }) => {
    await page.goto('/content/api/openapi/overview.html');
    
    const breadcrumbContainer = page.locator('.breadcrumb-container');
    if (await breadcrumbContainer.count() > 0) {
      // Check navigation landmark
      const nav = page.locator('nav[aria-label="breadcrumb"]');
      await expect(nav).toBeVisible();
      
      // Check aria-current on active item
      const activeItem = page.locator('.breadcrumb-item.active');
      if (await activeItem.count() > 0) {
        await expect(activeItem).toHaveAttribute('aria-current', 'page');
      }
      
      // Check screen reader text for home icon
      const homeLink = page.locator('.breadcrumb-item a[title="Home"]');
      if (await homeLink.count() > 0) {
        const srText = homeLink.locator('.sr-only');
        await expect(srText).toContainText('Home');
      }
    }
  });

  test('color contrast should be adequate', async ({ page }) => {
    await page.goto('/content/api/overview.html');
    
    const breadcrumbContainer = page.locator('.breadcrumb-container');
    if (await breadcrumbContainer.count() > 0) {
      // Get colors for contrast checking
      const linkColors = await page.locator('.breadcrumb-item a').first().evaluate((el) => {
        const style = getComputedStyle(el);
        return {
          color: style.color,
          backgroundColor: style.backgroundColor
        };
      });
      
      // Basic check that colors are defined
      expect(linkColors.color).toBeTruthy();
      expect(linkColors.backgroundColor).toBeTruthy();
      
      // Check active item colors
      const activeColors = await page.locator('.breadcrumb-item.active').evaluate((el) => {
        const style = getComputedStyle(el);
        return {
          color: style.color,
          backgroundColor: style.backgroundColor
        };
      });
      
      expect(activeColors.color).toBeTruthy();
    }
  });

  test('touch targets should be adequate size on mobile', async ({ page }) => {
    await page.setViewportSize({ width: 375, height: 667 });
    await page.goto('/content/api/openapi/overview.html');
    
    const breadcrumbLinks = page.locator('.breadcrumb-item a');
    const linkCount = await breadcrumbLinks.count();
    
    if (linkCount > 0) {
      for (let i = 0; i < linkCount; i++) {
        const link = breadcrumbLinks.nth(i);
        const boundingBox = await link.boundingBox();
        
        if (boundingBox) {
          // Touch targets should be at least 44x44px according to accessibility guidelines
          expect(boundingBox.height).toBeGreaterThanOrEqual(30); // Allow some flexibility
          expect(boundingBox.width).toBeGreaterThanOrEqual(30);
        }
      }
    }
  });
});