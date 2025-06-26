import { test, expect } from '@playwright/test';

test.describe('Integration Tests with Real Backend', () => {
	test('works with real backend - basic navigation', async ({ page }) => {
		// Start at home page
		await page.goto('/');
		await expect(page).toHaveTitle(/Billing Service/);
		
		// Navigate to cashiers using the navigation link
		await page.getByRole('link', { name: 'Cashiers' }).click();
		await expect(page).toHaveURL('/cashiers');
		
		// Page should load successfully (even if no cashiers exist)
		await expect(page.getByRole('heading', { name: 'Cashiers', exact: true })).toBeVisible();
		
		// Navigate to create form - try both possible buttons
		const addCashierButton = page.getByRole('button', { name: 'Add Cashier' });
		const createFirstButton = page.getByRole('button', { name: 'Create First Cashier' });
		
		if (await addCashierButton.isVisible()) {
			await addCashierButton.click();
		} else {
			await createFirstButton.click();
		}
		await expect(page).toHaveURL('/cashiers/create');
		
		// Verify form elements
		await expect(page.getByLabel('Name *')).toBeVisible();
		await expect(page.getByLabel('Email *')).toBeVisible();
	});

	test('cashiers page loads correctly', async ({ page }) => {
		await page.goto('/cashiers');
		await page.waitForLoadState('networkidle');
		
		// Verify page loads correctly
		await expect(page).toHaveTitle(/Cashiers - Billing Service/);
		await expect(page.getByRole('heading', { name: 'Cashiers', exact: true })).toBeVisible();
		
		// Should have either cashiers or empty state
		const hasContent = await page.getByText('Add Cashier').isVisible();
		expect(hasContent).toBe(true);
	});

	test('create cashier form works', async ({ page }) => {
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
});