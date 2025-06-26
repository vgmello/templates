import { test, expect } from '@playwright/test';
import { mockApiResponses } from './test-helpers';

test.describe('Mock API Tests', () => {
	test('works with mocked cashiers data', async ({ page }) => {
		// Set up API mocks before navigation
		await mockApiResponses(page, {
			cashiers: [
				{
					cashierId: "mock-id-1",
					name: "Mock Cashier",
					email: "mock@example.com",
					cashierPayments: [
						{ currency: "USD", isActive: true, createdDateUtc: new Date().toISOString() }
					],
					createdDateUtc: new Date().toISOString(),
					updatedDateUtc: new Date().toISOString(),
					version: 1
				}
			]
		});
		
		// Navigate to cashiers page
		await page.goto('/cashiers');
		await page.waitForLoadState('networkidle');
		
		// Verify page loads correctly
		await expect(page).toHaveTitle(/Cashiers - Billing Service/);
		await expect(page.getByRole('heading', { name: 'Cashiers', exact: true })).toBeVisible();
		
		// Verify mocked data appears
		await expect(page.getByText('Mock Cashier')).toBeVisible();
		await expect(page.getByText('mock@example.com')).toBeVisible();
	});

	test('works with empty cashiers data', async ({ page }) => {
		// Mock empty response
		await mockApiResponses(page, { cashiers: [] });
		
		// Navigate to cashiers page
		await page.goto('/cashiers');
		await page.waitForLoadState('networkidle');
		
		// Should show empty state
		await expect(page.getByText('No cashiers found')).toBeVisible();
		await expect(page.getByRole('button', { name: 'Add Cashier' })).toBeVisible();
	});

	test('basic navigation works', async ({ page }) => {
		// Start at home page
		await page.goto('/');
		await expect(page).toHaveTitle(/Billing Service/);
		
		// Navigate to cashiers - use exact text match
		await page.getByText('Manage Cashiers', { exact: true }).click();
		await expect(page).toHaveURL('/cashiers');
		
		// Navigate to create form
		await page.getByRole('button', { name: 'Add Cashier' }).click();
		await expect(page).toHaveURL('/cashiers/create');
		
		// Verify form elements
		await expect(page.getByLabel('Name *')).toBeVisible();
		await expect(page.getByLabel('Email *')).toBeVisible();
	});
});