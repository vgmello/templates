import { test, expect } from '@playwright/test';

test.describe('Create Cashier Form', () => {
	test.beforeEach(async ({ page }) => {
		await page.goto('/cashiers/create');
	});

	test('displays create cashier form correctly', async ({ page }) => {
		// Check page title
		await expect(page).toHaveTitle(/Create Cashier - Billing Service/);
		
		// Check main heading
		await expect(page.getByText('Create New Cashier')).toBeVisible();
		
		// Check subtitle
		await expect(page.getByText('Add a new cashier to handle payments for your business')).toBeVisible();
		
		// Check form sections
		await expect(page.getByText('Basic Information')).toBeVisible();
		await expect(page.getByText('Supported Currencies')).toBeVisible();
	});

	test('has all required form fields', async ({ page }) => {
		// Check Name field
		await expect(page.getByLabel('Name *')).toBeVisible();
		await expect(page.locator('#name')).toBeVisible();
		
		// Check Email field
		await expect(page.getByLabel('Email *')).toBeVisible();
		await expect(page.locator('#email')).toBeVisible();
		
		// Check currency input
		await expect(page.getByPlaceholder('Add currency (e.g., USD)')).toBeVisible();
		
		// Check default USD currency is present
		await expect(page.getByText('USD')).toBeVisible();
	});

	test('allows filling out basic information', async ({ page }) => {
		// Fill name field
		await page.getByLabel('Name *').fill('John Smith');
		await expect(page.getByLabel('Name *')).toHaveValue('John Smith');
		
		// Fill email field
		await page.getByLabel('Email *').fill('john.smith@example.com');
		await expect(page.getByLabel('Email *')).toHaveValue('john.smith@example.com');
	});

	test('currency management works correctly', async ({ page }) => {
		// USD should be present by default
		await expect(page.getByText('USD')).toBeVisible();
		
		// Add a new currency manually
		await page.getByPlaceholder('Add currency (e.g., USD)').fill('EUR');
		await page.getByRole('button').filter({ hasText: '+' }).click();
		
		// EUR should now be visible
		await expect(page.getByText('EUR')).toBeVisible();
		
		// Add another currency using quick-add buttons
		const gbpButton = page.getByRole('button', { name: 'GBP' });
		if (await gbpButton.isVisible()) {
			await gbpButton.click();
			await expect(page.getByText('GBP')).toBeVisible();
		}
	});

	test('can remove currencies', async ({ page }) => {
		// Add EUR first
		await page.getByPlaceholder('Add currency (e.g., USD)').fill('EUR');
		await page.getByRole('button').filter({ hasText: '+' }).click();
		
		// Find and click the remove button for EUR (X button)
		const eurBadge = page.locator('.gap-1:has-text("EUR")');
		await expect(eurBadge).toBeVisible();
		
		// Click the X button within the EUR badge
		await eurBadge.getByRole('button').click();
		
		// EUR should be removed
		await expect(page.getByText('EUR')).not.toBeVisible();
		
		// USD should still be present
		await expect(page.getByText('USD')).toBeVisible();
	});

	test('validates required fields', async ({ page }) => {
		// Remove the default USD currency first to trigger the validation
		const usdBadge = page.locator('.gap-1:has-text("USD")');
		await usdBadge.getByRole('button').click();
		
		// Try to submit empty form
		await page.getByRole('button', { name: 'Create Cashier' }).click();
		
		// Should show validation error - using actual error message
		await expect(page.getByText('Please fill in all required fields and add at least one currency.')).toBeVisible();
		
		// Fill only name
		await page.getByLabel('Name *').fill('Test Name');
		await page.getByRole('button', { name: 'Create Cashier' }).click();
		
		// Should still show validation error
		await expect(page.getByText('Please fill in all required fields and add at least one currency.')).toBeVisible();
	});

	test('form submission shows loading state', async ({ page }) => {
		// Fill required fields
		await page.getByLabel('Name *').fill('Test Cashier');
		await page.getByLabel('Email *').fill('test@example.com');
		
		// Submit form
		await page.getByRole('button', { name: 'Create Cashier' }).click();
		
		// Should show loading state
		await expect(page.getByText('Creating...')).toBeVisible();
		
		// Submit button should be disabled during loading
		await expect(page.getByRole('button', { name: 'Creating...' })).toBeDisabled();
	});

	test('back button works correctly', async ({ page }) => {
		// Click back button (arrow left icon)
		await page.getByRole('button').first().click();
		
		// Should navigate back to cashiers list
		await expect(page).toHaveURL('/cashiers');
		await expect(page.locator('h1')).toContainText('Cashiers');
	});

	test('cancel button works correctly', async ({ page }) => {
		// Fill some data first
		await page.getByLabel('Name *').fill('Test Data');
		
		// Click cancel button
		await page.getByRole('button', { name: 'Cancel' }).click();
		
		// Should navigate back to cashiers list
		await expect(page).toHaveURL('/cashiers');
		await expect(page.locator('h1')).toContainText('Cashiers');
	});

	test('quick add currency buttons work', async ({ page }) => {
		// Check that quick add buttons are present
		await expect(page.getByText('Quick add popular currencies:')).toBeVisible();
		
		// Try to add EUR using quick button
		const eurQuickButton = page.getByRole('button', { name: 'EUR' });
		if (await eurQuickButton.isVisible()) {
			await eurQuickButton.click();
			
			// EUR should be added to the currency list
			await expect(page.locator('.gap-1:has-text("EUR")')).toBeVisible();
			
			// EUR quick button should disappear (already added)
			await expect(eurQuickButton).not.toBeVisible();
		}
	});

	test('prevents duplicate currencies', async ({ page }) => {
		// Try to add USD again (it's already present by default)
		await page.getByPlaceholder('Add currency (e.g., USD)').fill('USD');
		await page.getByRole('button').filter({ hasText: '+' }).click();
		
		// Should not add duplicate USD
		const usdBadges = page.locator('.gap-1:has-text("USD")');
		await expect(usdBadges).toHaveCount(1);
	});

	test('handles keyboard interaction', async ({ page }) => {
		// Focus on currency input
		await page.getByPlaceholder('Add currency (e.g., USD)').focus();
		
		// Type EUR
		await page.getByPlaceholder('Add currency (e.g., USD)').fill('EUR');
		
		// Press Enter to add currency
		await page.getByPlaceholder('Add currency (e.g., USD)').press('Enter');
		
		// EUR should be added
		await expect(page.getByText('EUR')).toBeVisible();
		
		// Input should be cleared
		await expect(page.getByPlaceholder('Add currency (e.g., USD)')).toHaveValue('');
	});
});

test.describe('Form Accessibility', () => {
	test('has proper labels and form structure', async ({ page }) => {
		await page.goto('/cashiers/create');
		
		// Check form has proper labels
		await expect(page.getByLabel('Name *')).toBeVisible();
		await expect(page.getByLabel('Email *')).toBeVisible();
		
		// Check form is keyboard accessible
		await page.keyboard.press('Tab');
		await expect(page.getByLabel('Name *')).toBeFocused();
		
		await page.keyboard.press('Tab');
		await expect(page.getByLabel('Email *')).toBeFocused();
	});

	test('provides appropriate error messages', async ({ page }) => {
		await page.goto('/cashiers/create');
		
		// Submit empty form
		await page.getByRole('button', { name: 'Create Cashier' }).click();
		
		// Check error message is descriptive
		const errorMessage = page.getByText('Please fill in all required fields and add at least one currency');
		await expect(errorMessage).toBeVisible();
	});
});

test.describe('Form Responsive Design', () => {
	test('works on mobile devices', async ({ page }) => {
		await page.setViewportSize({ width: 375, height: 667 });
		await page.goto('/cashiers/create');
		
		// Form should be accessible on mobile
		await expect(page.getByText('Create New Cashier')).toBeVisible();
		await expect(page.getByLabel('Name *')).toBeVisible();
		await expect(page.getByLabel('Email *')).toBeVisible();
		
		// Should be able to interact with form elements
		await page.getByLabel('Name *').fill('Mobile Test');
		await expect(page.getByLabel('Name *')).toHaveValue('Mobile Test');
	});

	test('currency badges wrap properly on small screens', async ({ page }) => {
		await page.setViewportSize({ width: 320, height: 568 });
		await page.goto('/cashiers/create');
		
		// Add multiple currencies
		const currencies = ['EUR', 'GBP', 'CAD'];
		for (const currency of currencies) {
			await page.getByPlaceholder('Add currency (e.g., USD)').fill(currency);
			await page.getByRole('button').filter({ hasText: '+' }).click();
		}
		
		// All currencies should be visible even on very small screen
		await expect(page.getByText('USD')).toBeVisible();
		await expect(page.getByText('EUR')).toBeVisible();
		await expect(page.getByText('GBP')).toBeVisible();
		await expect(page.getByText('CAD')).toBeVisible();
	});
});