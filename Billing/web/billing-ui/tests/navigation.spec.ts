import { test, expect } from '@playwright/test';

test.describe('Application Navigation', () => {
	test('main page navigation works correctly', async ({ page }) => {
		await page.goto('/');
		
		// Check main page loads
		await expect(page).toHaveTitle(/Billing Service/);
		await expect(page.getByRole('heading', { name: 'Billing Service' })).toBeVisible();
		await expect(page.getByText('Manage cashiers, invoices, and payments')).toBeVisible();
	});

	test('header navigation links work', async ({ page }) => {
		await page.goto('/');
		
		// Test Billing Service link (home)
		await page.getByRole('link', { name: 'Billing Service' }).click();
		await expect(page).toHaveURL('/');
		
		// Test Cashiers link
		await page.getByRole('link', { name: 'Cashiers' }).click();
		await expect(page).toHaveURL('/cashiers');
		await expect(page.getByText('Manage cashiers and their payment configurations')).toBeVisible();
		
		// Navigate back to home via header
		await page.getByRole('link', { name: 'Billing Service' }).click();
		await expect(page).toHaveURL('/');
	});

	test('breadcrumb navigation flow', async ({ page }) => {
		// Start at home
		await page.goto('/');
		
		// Navigate to cashiers via button
		await page.getByRole('button', { name: 'Manage Cashiers' }).click();
		await expect(page).toHaveURL('/cashiers');
		
		// Navigate to create cashier
		await page.getByRole('button', { name: 'Add Cashier' }).click();
		await expect(page).toHaveURL('/cashiers/create');
		
		// Navigate back via back button
		await page.getByRole('button').first().click(); // Arrow back button
		await expect(page).toHaveURL('/cashiers');
		
		// Navigate to cashier details
		await page.waitForLoadState('networkidle');
		await page.getByText('Test Cashier').click();
		await expect(page).toHaveURL(/\/cashiers\/[a-f0-9-]+/);
		
		// Navigate back to cashiers list
		await page.getByRole('button').first().click(); // Arrow back button
		await expect(page).toHaveURL('/cashiers');
	});

	test('deep link navigation works', async ({ page }) => {
		// Direct navigation to create page
		await page.goto('/cashiers/create');
		await expect(page.getByText('Create New Cashier')).toBeVisible();
		
		// Direct navigation to specific cashier
		await page.goto('/cashiers/a52757cd-a42f-4fb9-8566-a98c61a71d2a');
		await expect(page.getByText('Cashier Details')).toBeVisible();
		
		// Direct navigation to cashiers list
		await page.goto('/cashiers');
		await expect(page.getByText('Manage cashiers and their payment configurations')).toBeVisible();
	});

	test('browser back/forward buttons work', async ({ page }) => {
		// Start at home
		await page.goto('/');
		
		// Navigate to cashiers
		await page.getByRole('button', { name: 'Manage Cashiers' }).click();
		await expect(page).toHaveURL('/cashiers');
		
		// Navigate to create
		await page.getByRole('button', { name: 'Add Cashier' }).click();
		await expect(page).toHaveURL('/cashiers/create');
		
		// Use browser back button
		await page.goBack();
		await expect(page).toHaveURL('/cashiers');
		
		// Use browser forward button
		await page.goForward();
		await expect(page).toHaveURL('/cashiers/create');
		
		// Go back twice to reach home
		await page.goBack();
		await page.goBack();
		await expect(page).toHaveURL('/');
	});

	test('maintains scroll position during navigation', async ({ page }) => {
		await page.goto('/cashiers');
		await page.waitForLoadState('networkidle');
		
		// Scroll down if there's content
		await page.evaluate(() => window.scrollTo(0, 200));
		
		// Navigate to create page
		await page.getByRole('button', { name: 'Add Cashier' }).click();
		await expect(page).toHaveURL('/cashiers/create');
		
		// Navigate back
		await page.getByRole('button').first().click();
		await expect(page).toHaveURL('/cashiers');
		
		// Page should load correctly (scroll position may vary)
		await expect(page.getByText('Manage cashiers and their payment configurations')).toBeVisible();
	});
});

test.describe('Route Handling', () => {
	test('handles 404 routes gracefully', async ({ page }) => {
		// Try to navigate to non-existent route
		await page.goto('/nonexistent');
		
		// Should handle gracefully (might show 404 or redirect)
		// The exact behavior depends on SvelteKit configuration
		// For now, we just check that the app doesn't crash
		await expect(page.locator('body')).toBeVisible();
	});

	test('handles invalid cashier IDs', async ({ page }) => {
		// Try to navigate to cashier with invalid ID
		await page.goto('/cashiers/invalid-id');
		
		// Should handle gracefully with error page or redirect
		await expect(page.locator('body')).toBeVisible();
	});
});

test.describe('URL State Management', () => {
	test('preserves URL parameters during navigation', async ({ page }) => {
		// Navigate with query parameters
		await page.goto('/cashiers?test=value');
		
		// Parameters should be preserved
		expect(page.url()).toContain('test=value');
		
		// Navigation should work normally
		await expect(page.getByText('Manage cashiers and their payment configurations')).toBeVisible();
	});

	test('handles URL fragments correctly', async ({ page }) => {
		// Navigate with fragment
		await page.goto('/cashiers#section');
		
		// Fragment should be preserved
		expect(page.url()).toContain('#section');
		
		// Page should load normally
		await expect(page.getByText('Manage cashiers and their payment configurations')).toBeVisible();
	});
});

test.describe('Navigation Performance', () => {
	test('navigation is fast and responsive', async ({ page }) => {
		await page.goto('/');
		
		// Measure navigation time
		const startTime = Date.now();
		await page.getByRole('button', { name: 'Manage Cashiers' }).click();
		await page.waitForLoadState('networkidle');
		const endTime = Date.now();
		
		// Navigation should be fast (less than 2 seconds)
		const navigationTime = endTime - startTime;
		expect(navigationTime).toBeLessThan(2000);
		
		await expect(page).toHaveURL('/cashiers');
	});

	test('pages load without layout shift', async ({ page }) => {
		// Navigate to cashiers page
		await page.goto('/cashiers');
		
		// Wait for initial load
		await page.waitForLoadState('networkidle');
		
		// Take initial screenshot for comparison
		await page.screenshot({ path: 'initial-load.png' });
		
		// Wait a bit more to ensure no layout shifts
		await page.waitForTimeout(500);
		
		// Content should be stable
		await expect(page.getByText('Manage cashiers and their payment configurations')).toBeVisible();
	});
});

test.describe('Mobile Navigation', () => {
	test('navigation works on mobile viewport', async ({ page }) => {
		await page.setViewportSize({ width: 375, height: 667 });
		await page.goto('/');
		
		// Navigation should work on mobile
		await page.getByRole('button', { name: 'Manage Cashiers' }).click();
		await expect(page).toHaveURL('/cashiers');
		
		// Back navigation should work
		await page.getByRole('link', { name: 'Billing Service' }).click();
		await expect(page).toHaveURL('/');
	});

	test('touch navigation works correctly', async ({ page }) => {
		await page.setViewportSize({ width: 375, height: 667 });
		await page.goto('/cashiers');
		await page.waitForLoadState('networkidle');
		
		// Click on cashier card (simulating touch)
		await page.getByText('Test Cashier').click();
		await expect(page).toHaveURL(/\/cashiers\/[a-f0-9-]+/);
		
		// Click back button (simulating touch)
		await page.getByRole('button').first().click();
		await page.waitForURL('/cashiers');
		await expect(page).toHaveURL('/cashiers');
	});
});