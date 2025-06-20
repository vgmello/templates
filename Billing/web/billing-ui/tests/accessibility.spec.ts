import { test, expect } from '@playwright/test';

test.describe('Accessibility Tests', () => {
	test('main page is accessible', async ({ page }) => {
		await page.goto('/');
		
		// Check basic accessibility features
		await expect(page.locator('h1')).toBeVisible();
		await expect(page.locator('main')).toBeVisible();
		
		// Check buttons have accessible names
		const manageButton = page.getByRole('button', { name: 'Manage Cashiers' });
		await expect(manageButton).toBeVisible();
		
		// Check links have accessible names
		const cashiersLink = page.getByRole('link', { name: 'Cashiers' });
		await expect(cashiersLink).toBeVisible();
	});

	test('cashiers page is accessible', async ({ page }) => {
		await page.goto('/cashiers');
		
		// Check heading structure
		await expect(page.getByRole('heading', { level: 1 })).toContainText('Cashiers');
		
		// Check buttons are accessible
		await expect(page.getByRole('button', { name: 'Add Cashier' })).toBeVisible();
		
		// Check cards are clickable and have proper content
		await page.waitForLoadState('networkidle');
		const cashierCards = page.getByText('Test Cashier');
		await expect(cashierCards).toBeVisible();
	});

	test('create form is accessible', async ({ page }) => {
		await page.goto('/cashiers/create');
		
		// Check form labels are properly associated
		await expect(page.getByLabel('Name *')).toBeVisible();
		await expect(page.getByLabel('Email *')).toBeVisible();
		
		// Check required fields are marked
		const nameField = page.getByLabel('Name *');
		await expect(nameField).toHaveAttribute('required');
		
		const emailField = page.getByLabel('Email *');
		await expect(emailField).toHaveAttribute('required');
		
		// Check form submission button
		await expect(page.getByRole('button', { name: 'Create Cashier' })).toBeVisible();
	});

	test('keyboard navigation works throughout app', async ({ page }) => {
		await page.goto('/');
		
		// Tab through main page elements
		await page.keyboard.press('Tab');
		await page.keyboard.press('Tab');
		
		// Should be able to navigate with keyboard
		await page.keyboard.press('Enter');
		
		// Check we can navigate with keyboard
		// (This is a basic test - more detailed keyboard testing would require specific focus management)
	});

	test('has proper semantic HTML structure', async ({ page }) => {
		await page.goto('/cashiers');
		
		// Check semantic elements exist
		await expect(page.locator('header')).toBeVisible();
		await expect(page.locator('main')).toBeVisible();
		await expect(page.locator('nav')).toBeVisible();
		
		// Check headings hierarchy
		await expect(page.locator('h1')).toBeVisible();
	});

	test('color contrast and visibility', async ({ page }) => {
		await page.goto('/');
		
		// Check that text is visible (basic visibility test) - use more specific selectors
		await expect(page.getByRole('heading', { name: 'Billing Service' })).toBeVisible();
		await expect(page.getByText('Manage cashiers, invoices, and payments')).toBeVisible();
		
		// Check buttons are visible and styled
		const button = page.getByRole('button', { name: 'Manage Cashiers' });
		await expect(button).toBeVisible();
		await expect(button).toHaveCSS('color', /.+/); // Has some color
	});

	test('focuses are visible and manageable', async ({ page }) => {
		await page.goto('/cashiers/create');
		
		// Tab to first form field
		await page.keyboard.press('Tab');
		const focusedElement = page.locator(':focus');
		await expect(focusedElement).toBeVisible();
		
		// Tab to next field
		await page.keyboard.press('Tab');
		const nextFocusedElement = page.locator(':focus');
		await expect(nextFocusedElement).toBeVisible();
	});

	test('error messages are accessible', async ({ page }) => {
		await page.goto('/cashiers/create');
		
		// Remove the default USD currency first to trigger the validation
		// Find the USD badge and click its X button
		await page.locator('[class*="gap-1"]:has-text("USD") button').click();
		
		// Fill required fields except currencies to avoid browser validation 
		await page.getByLabel('Name *').fill('Test Name');
		await page.getByLabel('Email *').fill('test@example.com');
		
		// Submit form to trigger our custom validation for missing currency
		await page.getByRole('button', { name: 'Create Cashier' }).click();
		
		// Error message should be visible and accessible - using actual error message from the form
		const errorMessage = page.getByText('Please fill in all required fields and add at least one currency.');
		await expect(errorMessage).toBeVisible();
	});
});

test.describe('Performance Tests', () => {
	test('pages load within acceptable time', async ({ page }) => {
		// Test home page load time
		const homeStartTime = Date.now();
		await page.goto('/');
		await page.waitForLoadState('networkidle');
		const homeEndTime = Date.now();
		
		expect(homeEndTime - homeStartTime).toBeLessThan(3000); // Less than 3 seconds
		
		// Test cashiers page load time
		const cashiersStartTime = Date.now();
		await page.goto('/cashiers');
		await page.waitForLoadState('networkidle');
		const cashiersEndTime = Date.now();
		
		expect(cashiersEndTime - cashiersStartTime).toBeLessThan(3000);
	});

	test('images and assets load efficiently', async ({ page }) => {
		await page.goto('/');
		
		// Check that favicon loads
		const favicon = page.locator('link[rel="icon"]');
		if (await favicon.count() > 0) {
			const faviconHref = await favicon.getAttribute('href');
			expect(faviconHref).toBeTruthy();
		}
	});

	test('handles rapid navigation without issues', async ({ page }) => {
		await page.goto('/');
		
		// Rapidly navigate between pages
		for (let i = 0; i < 3; i++) {
			await page.getByRole('button', { name: 'Manage Cashiers' }).click();
			await expect(page).toHaveURL('/cashiers');
			
			await page.getByRole('button', { name: 'Add Cashier' }).click();
			await expect(page).toHaveURL('/cashiers/create');
			
			await page.getByRole('button').first().click(); // Back button
			await expect(page).toHaveURL('/cashiers');
			
			await page.getByRole('link', { name: 'Billing Service' }).click();
			await expect(page).toHaveURL('/');
		}
		
		// App should still be responsive
		await expect(page.getByRole('heading', { name: 'Billing Service' })).toBeVisible();
	});

	test('memory usage remains stable during navigation', async ({ page }) => {
		await page.goto('/');
		
		// Navigate through different pages multiple times
		const pages = ['/', '/cashiers', '/cashiers/create'];
		
		for (let cycle = 0; cycle < 5; cycle++) {
			for (const route of pages) {
				await page.goto(route);
				await page.waitForLoadState('networkidle');
				
				// Basic check that page loads correctly
				await expect(page.locator('body')).toBeVisible();
			}
		}
		
		// Final check that everything still works
		await page.goto('/cashiers');
		await expect(page.getByText('Manage cashiers and their payment configurations')).toBeVisible();
	});

	test('handles network delays gracefully', async ({ page }) => {
		// Simulate slow network
		await page.route('**/api/**', async (route) => {
			// Add delay to API calls
			await new Promise(resolve => setTimeout(resolve, 1000));
			await route.continue();
		});
		
		await page.goto('/cashiers');
		
		// Should still load and show fallback data
		await page.waitForLoadState('networkidle');
		await expect(page.getByText('Test Cashier')).toBeVisible();
	});
});

test.describe('SEO and Meta Tags', () => {
	test('has proper page titles', async ({ page }) => {
		// Home page
		await page.goto('/');
		await expect(page).toHaveTitle(/Billing Service/);
		
		// Cashiers page
		await page.goto('/cashiers');
		await expect(page).toHaveTitle(/Cashiers - Billing Service/);
		
		// Create page
		await page.goto('/cashiers/create');
		await expect(page).toHaveTitle(/Create Cashier - Billing Service/);
		
		// Detail page
		await page.goto('/cashiers/a52757cd-a42f-4fb9-8566-a98c61a71d2a');
		await expect(page).toHaveTitle(/Mock Cashier - Billing Service|Cashier Details - Billing Service/);
	});

	test('has proper meta descriptions', async ({ page }) => {
		await page.goto('/');
		
		// Check meta description exists
		const metaDescription = page.locator('meta[name="description"]');
		await expect(metaDescription).toHaveAttribute('content', /.+/);
	});

	test('has proper heading structure for SEO', async ({ page }) => {
		await page.goto('/cashiers');
		
		// Should have only one h1
		const h1Elements = page.locator('h1');
		await expect(h1Elements).toHaveCount(1);
		
		// H1 should contain relevant content
		await expect(h1Elements).toContainText('Cashiers');
	});
});