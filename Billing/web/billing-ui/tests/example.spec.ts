import { test, expect } from '@playwright/test';

// Simple example test demonstrating core patterns
test.describe('Example Tests - Getting Started', () => {
	test('basic page load and interaction', async ({ page }) => {
		// Navigate to the main page
		await page.goto('/');
		
		// Verify page loads correctly
		await expect(page).toHaveTitle(/Billing Service/);
		await expect(page.getByRole('heading', { name: 'Billing Service' })).toBeVisible();
		
		// Test navigation - click the cashiers card and verify navigation
		await page.getByText('Manage Cashiers', { exact: true }).click();
		await page.waitForURL('/cashiers', { timeout: 10000 });
		await expect(page).toHaveURL('/cashiers');
		
		// Verify content loads
		await page.waitForLoadState('networkidle');
		await expect(page.getByText('Manage cashiers and their payment configurations')).toBeVisible();
	});

	test('form interaction example', async ({ page }) => {
		// Navigate to create form
		await page.goto('/cashiers/create');
		
		// Fill out form
		await page.getByLabel('Name *').fill('Test User');
		await page.getByLabel('Email *').fill('test@example.com');
		
		// Verify form state
		await expect(page.getByLabel('Name *')).toHaveValue('Test User');
		await expect(page.getByLabel('Email *')).toHaveValue('test@example.com');
		
		// Test currency addition using quick-add button
		await page.getByRole('button', { name: 'EUR' }).click();
		await expect(page.getByText('EUR')).toBeVisible();
	});

	test('responsive design example', async ({ page }) => {
		// Test on mobile viewport
		await page.setViewportSize({ width: 375, height: 667 });
		await page.goto('/cashiers');
		
		// Content should be visible and accessible - use exact match to avoid conflicts
		await expect(page.getByRole('heading', { name: 'Cashiers', exact: true })).toBeVisible();
		await expect(page.getByRole('button', { name: 'Add Cashier' })).toBeVisible();
	});

	test('accessibility example', async ({ page }) => {
		await page.goto('/cashiers/create');
		
		// Test form labels exist
		await expect(page.getByLabel('Name *')).toBeVisible();
		await expect(page.getByLabel('Email *')).toBeVisible();
		
		// Test clicking on labels focuses the inputs
		await page.getByText('Name *').click();
		await expect(page.getByLabel('Name *')).toBeFocused();
		
		await page.getByText('Email *').click();
		await expect(page.getByLabel('Email *')).toBeFocused();
	});

	test('error handling example', async ({ page }) => {
		await page.goto('/cashiers/create');
		
		// Clear the name field (make it empty) and try to submit
		await page.getByLabel('Name *').clear();
		await page.getByLabel('Email *').clear();
		
		// Try to submit form with missing fields
		await page.getByRole('button', { name: 'Create Cashier' }).click();
		
		// Check for any validation error message - look for browser validation or form errors
		const hasValidationMessage = page.locator('[data-testid="validation-error"]').or(
			page.locator('input:invalid')
		);
		await expect(hasValidationMessage.first()).toBeVisible({ timeout: 10000 });
	});
});