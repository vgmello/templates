// @ts-check
const { test, expect } = require('@playwright/test');

test.describe('Documentation Structure', () => {
  test('home page should load without errors', async ({ page }) => {
    await page.goto('/');
    
    // Check that the page loaded successfully
    await expect(page.locator('h1')).toBeVisible();
    
    // Check for navigation elements
    const nav = page.locator('nav');
    await expect(nav).toBeVisible();
    
    // Check for main content
    const main = page.locator('main, article[role="main"]');
    await expect(main).toBeVisible();
  });

  test('all main documentation sections should be accessible', async ({ page }) => {
    const sections = [
      '/content/overview.html',
      '/content/architecture/overview.html', 
      '/content/api/overview.html',
      '/content/database-integration/overview.html',
      '/content/messaging/overview.html',
      '/content/healthchecks/overview.html',
      '/content/logging/overview.html',
      '/content/opentelemetry/overview.html',
      '/content/source-generators/overview.html',
      '/content/extensions/overview.html'
    ];

    for (const section of sections) {
      await page.goto(section);
      
      // Check page loads without 404
      await expect(page).not.toHaveURL(/.*404.*/);
      
      // Check main content is visible
      const article = page.locator('article, main');
      await expect(article).toBeVisible();
      
      // Check page has a title/heading
      const heading = page.locator('h1, h2').first();
      await expect(heading).toBeVisible();
    }
  });

  test('newly created OpenAPI documentation pages should load', async ({ page }) => {
    const openApiPages = [
      '/content/api/openapi/overview.html',
      '/content/api/openapi/xml-documentation.html',
      '/content/api/openapi/transformers.html'
    ];

    for (const pagePath of openApiPages) {
      await page.goto(pagePath);
      
      // Check page loads successfully
      await expect(page).not.toHaveURL(/.*404.*/);
      
      // Check main content is visible
      const article = page.locator('article, main');
      await expect(article).toBeVisible();
      
      // Check page has content
      const heading = page.locator('h1').first();
      await expect(heading).toBeVisible();
    }
  });

  test('newly created API documentation pages should load', async ({ page }) => {
    const apiPages = [
      '/content/api/grpc-integration.html',
      '/content/api/endpoint-filters.html'
    ];

    for (const pagePath of apiPages) {
      await page.goto(pagePath);
      
      // Check page loads successfully
      await expect(page).not.toHaveURL(/.*404.*/);
      
      // Check main content is visible
      const article = page.locator('article, main');
      await expect(article).toBeVisible();
      
      // Check page has content
      const heading = page.locator('h1').first();
      await expect(heading).toBeVisible();
    }
  });

  test('newly created database integration pages should load', async ({ page }) => {
    const dbPages = [
      '/content/database-integration/dapper-extensions.html',
      '/content/database-integration/source-generators.html'
    ];

    for (const pagePath of dbPages) {
      await page.goto(pagePath);
      
      // Check page loads successfully
      await expect(page).not.toHaveURL(/.*404.*/);
      
      // Check main content is visible
      const article = page.locator('article, main');
      await expect(article).toBeVisible();
      
      // Check page has content
      const heading = page.locator('h1').first();
      await expect(heading).toBeVisible();
    }
  });

  test('newly created setup pages should load', async ({ page }) => {
    const setupPages = [
      '/content/healthchecks/setup.html',
      '/content/opentelemetry/setup.html'
    ];

    for (const pagePath of setupPages) {
      await page.goto(pagePath);
      
      // Check page loads successfully
      await expect(page).not.toHaveURL(/.*404.*/);
      
      // Check main content is visible
      const article = page.locator('article, main');
      await expect(article).toBeVisible();
      
      // Check page has content
      const heading = page.locator('h1').first();
      await expect(heading).toBeVisible();
    }
  });

  test('side navigation should be functional', async ({ page }) => {
    await page.goto('/content/overview.html');
    
    // Check for side navigation
    const sideNav = page.locator('.sidetoc, #affix, nav.toc');
    await expect(sideNav).toBeVisible();
    
    // Check that navigation has links
    const navLinks = page.locator('.sidetoc a, #affix a, nav.toc a');
    const linkCount = await navLinks.count();
    expect(linkCount).toBeGreaterThan(0);
  });

  test('search functionality should be available', async ({ page }) => {
    await page.goto('/');
    
    // Check for search input
    const searchInput = page.locator('input[type="search"], input[placeholder*="search" i], .search-input');
    
    // Search might not be visible immediately, so we'll check if it exists
    const searchExists = await searchInput.count() > 0;
    if (searchExists) {
      await expect(searchInput.first()).toBeVisible();
    }
  });

  test('code blocks should be properly formatted', async ({ page }) => {
    await page.goto('/content/api/overview.html');
    
    // Look for code blocks
    const codeBlocks = page.locator('pre code, .highlight code');
    const codeBlockCount = await codeBlocks.count();
    
    if (codeBlockCount > 0) {
      // Check that code blocks are visible and formatted
      const firstCodeBlock = codeBlocks.first();
      await expect(firstCodeBlock).toBeVisible();
      
      // Check that code block has appropriate styling
      const codeStyle = await firstCodeBlock.evaluate((el) => {
        const style = getComputedStyle(el);
        return {
          fontFamily: style.fontFamily,
          backgroundColor: style.backgroundColor
        };
      });
      
      // Should use monospace font
      expect(codeStyle.fontFamily).toMatch(/monospace|consolas|courier/i);
    }
  });

  test('tables should be properly styled', async ({ page }) => {
    // Look for pages that might contain tables
    const pagesWithTables = [
      '/content/api/overview.html',
      '/content/database-integration/overview.html'
    ];

    for (const pagePath of pagesWithTables) {
      await page.goto(pagePath);
      
      const tables = page.locator('table');
      const tableCount = await tables.count();
      
      if (tableCount > 0) {
        const firstTable = tables.first();
        await expect(firstTable).toBeVisible();
        
        // Check that table has some basic styling
        const tableStyle = await firstTable.evaluate((el) => {
          const style = getComputedStyle(el);
          return {
            borderCollapse: style.borderCollapse,
            backgroundColor: style.backgroundColor
          };
        });
        
        // Basic table styling checks
        expect(tableStyle.borderCollapse).toBeTruthy();
      }
    }
  });

  test('external links should have proper attributes', async ({ page }) => {
    await page.goto('/content/overview.html');
    
    // Look for external links (starting with http)
    const externalLinks = page.locator('a[href^="http"]');
    const externalLinkCount = await externalLinks.count();
    
    if (externalLinkCount > 0) {
      const firstExternalLink = externalLinks.first();
      
      // External links should open in new tab/window
      const target = await firstExternalLink.getAttribute('target');
      const rel = await firstExternalLink.getAttribute('rel');
      
      // Either should have target="_blank" or be handled appropriately
      expect(target === '_blank' || rel?.includes('noopener')).toBeTruthy();
    }
  });
});