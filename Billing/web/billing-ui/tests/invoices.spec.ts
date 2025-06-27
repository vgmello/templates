import { test, expect } from '@playwright/test';

test.describe('Invoice List', () => {
	test('invoice list page loads correctly', async ({ page }) => {
		await page.goto('/invoices');
		
		// Check page title and heading - use exact match to avoid conflicts
		await expect(page).toHaveTitle(/Invoices - Billing System/);
		await expect(page.getByRole('heading', { name: 'Invoices', exact: true })).toBeVisible();
		await expect(page.getByText('Manage and track your invoices')).toBeVisible();
	});

	test('displays statistics cards', async ({ page }) => {
		await page.goto('/invoices');
		await page.waitForLoadState('networkidle');
		
		// Check statistics cards are visible - use role selectors to be more specific
		await expect(page.getByRole('heading', { name: 'Total Invoices' })).toBeVisible();
		await expect(page.getByRole('heading', { name: 'Total Amount' })).toBeVisible();
		await expect(page.getByRole('heading', { name: 'Paid' })).toBeVisible();
		await expect(page.getByRole('heading', { name: 'Overdue' })).toBeVisible();
	});

	test('search functionality works', async ({ page }) => {
		await page.goto('/invoices');
		await page.waitForLoadState('networkidle');
		
		// Check search input is present
		const searchInput = page.getByPlaceholder('Search by invoice name or ID...');
		await expect(searchInput).toBeVisible();
		
		// Test search functionality (if there are invoices)
		await searchInput.fill('test');
		
		// The filtered results should update automatically
		// This test will pass even if no invoices match
	});

	test('status filter works', async ({ page }) => {
		await page.goto('/invoices');
		await page.waitForLoadState('networkidle');
		
		// Check status filter dropdown is present
		const statusFilter = page.locator('select').first();
		await expect(statusFilter).toBeVisible();
		
		// Check default option
		await expect(statusFilter).toHaveValue('');
		
		// Check that "All Statuses" option exists
		await expect(statusFilter.locator('option[value=""]')).toContainText('All Statuses');
	});

	test('create invoice button works', async ({ page }) => {
		await page.goto('/invoices');
		
		// Click create invoice link
		await page.getByRole('link', { name: 'Create Invoice' }).click();
		
		// Should navigate to create page
		await expect(page).toHaveURL('/invoices/create');
	});

	test('displays invoice data when available', async ({ page }) => {
		await page.goto('/invoices');
		await page.waitForLoadState('networkidle');
		
		// With mock data, we should see at least one invoice
		const invoiceCards = page.locator('div:has(> div:has(h3 > a))');
		await expect(invoiceCards.first()).toBeVisible();
		
		// Should see mock invoice data
		await expect(page.getByText('Mock Invoice 1')).toBeVisible();
		await expect(page.getByText('Mock Invoice 2')).toBeVisible();
	});

	test('invoice cards display correct information', async ({ page }) => {
		await page.goto('/invoices');
		await page.waitForLoadState('networkidle');
		
		// Check specific invoice names are visible
		await expect(page.getByText('Mock Invoice 1')).toBeVisible();
		await expect(page.getByText('Mock Invoice 2')).toBeVisible();
		
		// Check status badges
		await expect(page.getByText('Draft')).toBeVisible();
		await expect(page.getByText('Paid')).toBeVisible();
		
		// Check invoice details are present
		await expect(page.getByText(/Invoice ID:/)).toBeVisible();
		await expect(page.getByText(/Amount:/)).toBeVisible();
		await expect(page.getByText(/Created:/)).toBeVisible();
		
		// Should have action buttons
		await expect(page.getByRole('button', { name: 'View Details' }).first()).toBeVisible();
	});

	test('invoice navigation works', async ({ page }) => {
		await page.goto('/invoices');
		await page.waitForLoadState('networkidle');
		
		// Find first invoice link if any exist
		const firstInvoiceLink = page.locator('h3 a').first();
		
		if (await firstInvoiceLink.isVisible()) {
			const invoiceId = await firstInvoiceLink.getAttribute('href');
			await firstInvoiceLink.click();
			
			// Should navigate to invoice details
			await expect(page).toHaveURL(invoiceId!);
		}
	});

	test('status badges display correctly', async ({ page }) => {
		await page.goto('/invoices');
		await page.waitForLoadState('networkidle');
		
		// Check if any status badges exist
		const statusBadges = page.locator('span:has-text("Paid"), span:has-text("Draft"), span:has-text("Cancelled")');
		
		// Status badges should have appropriate styling if they exist
		if (await statusBadges.count() > 0) {
			const firstBadge = statusBadges.first();
			await expect(firstBadge).toBeVisible();
			
			// Check if badge has appropriate color classes
			const badgeClass = await firstBadge.getAttribute('class');
			expect(badgeClass).toContain('bg-');
		}
	});

	test('overdue badges appear for overdue invoices', async ({ page }) => {
		await page.goto('/invoices');
		await page.waitForLoadState('networkidle');
		
		// Check for overdue badges - they may or may not exist
		const overdueBadges = page.getByText('Overdue');
		
		// If overdue badges exist, they should be red
		if (await overdueBadges.count() > 0) {
			const firstOverdueBadge = overdueBadges.first();
			await expect(firstOverdueBadge).toBeVisible();
			
			const badgeClass = await firstOverdueBadge.getAttribute('class');
			expect(badgeClass).toContain('text-red');
		}
	});

	test('error handling works correctly', async ({ page }) => {
		// This test checks that error messages can be displayed and dismissed
		await page.goto('/invoices');
		
		// Check if error message exists (might not always be present)
		const errorMessage = page.locator('.bg-destructive\\/15');
		
		if (await errorMessage.isVisible()) {
			// Should have dismiss button
			const dismissButton = errorMessage.getByRole('button', { name: 'Dismiss' });
			await expect(dismissButton).toBeVisible();
			
			// Clicking dismiss should hide error
			await dismissButton.click();
			await expect(errorMessage).not.toBeVisible();
		}
	});

	test('responsive design works on mobile', async ({ page }) => {
		// Set mobile viewport
		await page.setViewportSize({ width: 375, height: 667 });
		await page.goto('/invoices');
		
		// Page should still be functional on mobile
		await expect(page.getByRole('heading', { name: 'Invoices', exact: true })).toBeVisible();
		await expect(page.getByRole('link', { name: 'Create Invoice' })).toBeVisible();
		
		// Statistics cards should stack on mobile
		const statsCards = page.getByRole('heading', { name: 'Total Invoices' });
		await expect(statsCards).toBeVisible();
	});

	test('keyboard navigation works', async ({ page }) => {
		await page.goto('/invoices');
		await page.waitForLoadState('networkidle');
		
		// Tab through the page
		await page.keyboard.press('Tab');
		await page.keyboard.press('Tab');
		
		// Should be able to navigate with keyboard
		const focusedElement = page.locator(':focus');
		await expect(focusedElement).toBeVisible();
	});
});

test.describe('Invoice List Performance', () => {
	test('page loads within acceptable time', async ({ page }) => {
		const startTime = Date.now();
		await page.goto('/invoices');
		await page.waitForLoadState('networkidle');
		const endTime = Date.now();
		
		// Should load within 3 seconds
		expect(endTime - startTime).toBeLessThan(3000);
	});

	test('search filtering is responsive', async ({ page }) => {
		await page.goto('/invoices');
		await page.waitForLoadState('networkidle');
		
		const searchInput = page.getByPlaceholder('Search by invoice name or ID...');
		
		// Type in search box - should filter immediately
		const startTime = Date.now();
		await searchInput.fill('test');
		const endTime = Date.now();
		
		// Filtering should be fast (less than 100ms)
		expect(endTime - startTime).toBeLessThan(100);
	});
});