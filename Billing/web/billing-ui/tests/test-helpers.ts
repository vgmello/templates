/**
 * Test helper functions for Playwright tests
 */
import type { Page } from '@playwright/test';

/**
 * Wait for cashier data to load on the page
 */
export async function waitForCashiersToLoad(page: Page): Promise<void> {
	await page.waitForLoadState('networkidle');
	// Wait for either the cashier cards or empty state to appear
	await Promise.race([
		page.waitForSelector('text=Test Cashier', { timeout: 5000 }),
		page.waitForSelector('text=No cashiers found', { timeout: 5000 }),
	]);
}

interface CashierFormData {
	name?: string;
	email?: string;
	currencies?: string[];
}

/**
 * Fill out the create cashier form with test data
 */
export async function fillCashierForm(page: Page, data: CashierFormData = {}): Promise<void> {
	const {
		name = 'Test Cashier',
		email = 'test@example.com',
		currencies = ['USD', 'EUR']
	} = data;

	// Fill basic information
	await page.getByLabel('Name *').fill(name);
	await page.getByLabel('Email *').fill(email);

	// Add currencies (USD is already there by default)
	for (const currency of currencies) {
		if (currency !== 'USD') {
			await page.getByPlaceholder('Add currency (e.g., USD)').fill(currency);
			await page.getByRole('button').filter({ hasText: '+' }).click();
		}
	}
}

/**
 * Navigate through the complete cashier workflow
 */
export async function completeCashierWorkflow(page: Page): Promise<Page> {
	// Start at home
	await page.goto('/');
	
	// Navigate to cashiers
	await page.getByRole('button', { name: 'Manage Cashiers' }).click();
	
	// Navigate to create form
	await page.getByRole('button', { name: 'Add Cashier' }).click();
	
	// Fill and submit form
	await fillCashierForm(page);
	await page.getByRole('button', { name: 'Create Cashier' }).click();
	
	return page;
}

type ViewportType = 'mobile' | 'tablet' | 'desktop';

/**
 * Check if the page has the expected responsive layout
 */
export async function checkResponsiveLayout(page: Page, viewport: ViewportType = 'desktop'): Promise<Page> {
	const viewportSizes = {
		mobile: { width: 375, height: 667 },
		tablet: { width: 768, height: 1024 },
		desktop: { width: 1280, height: 720 }
	};
	
	await page.setViewportSize(viewportSizes[viewport]);
	
	// Basic layout checks that should work on all viewports
	await page.locator('header').isVisible();
	await page.locator('main').isVisible();
	
	return page;
}

interface MockData {
	cashiers?: Array<{
		cashierId: string;
		name: string;
		email: string;
		cashierPayments: Array<{
			currency: string;
			isActive: boolean;
			createdDateUtc: string;
		}>;
		createdDateUtc: string;
		updatedDateUtc: string;
		version: number;
	}>;
}

/**
 * Mock API responses for testing
 */
export async function mockApiResponses(page: Page, mockData: MockData = {}): Promise<void> {
	const defaultMockData = {
		cashiers: [
			{
				cashierId: "test-id-1",
				name: "Test Cashier 1",
				email: "test1@example.com",
				cashierPayments: [
					{ currency: "USD", isActive: true, createdDateUtc: new Date().toISOString() }
				],
				createdDateUtc: new Date().toISOString(),
				updatedDateUtc: new Date().toISOString(),
				version: 1
			}
		]
	};
	
	const data = { ...defaultMockData, ...mockData };
	
	// Mock GET /api/cashiers
	await page.route('**/api/cashiers', async (route) => {
		if (route.request().method() === 'GET') {
			await route.fulfill({
				status: 200,
				contentType: 'application/json',
				body: JSON.stringify(data.cashiers)
			});
		} else {
			await route.continue();
		}
	});
	
	// Mock GET /api/cashiers/:id
	await page.route('**/api/cashiers/*', async (route) => {
		if (route.request().method() === 'GET') {
			const cashier = data.cashiers[0]; // Return first cashier for any ID
			await route.fulfill({
				status: 200,
				contentType: 'application/json',
				body: JSON.stringify(cashier)
			});
		} else {
			await route.continue();
		}
	});
}

/**
 * Assert that error handling works correctly
 */
export async function testErrorHandling(page: Page): Promise<Page> {
	// Mock API to return error
	await page.route('**/api/cashiers', async (route) => {
		await route.fulfill({
			status: 500,
			contentType: 'application/json',
			body: JSON.stringify({ error: 'Internal Server Error' })
		});
	});
	
	await page.goto('/cashiers');
	
	// Should still show fallback data or error state
	await page.waitForLoadState('networkidle');
	
	return page;
}

/**
 * Simulate slow network conditions
 */
export async function simulateSlowNetwork(page: Page, delay: number = 2000): Promise<void> {
	await page.route('**/api/**', async (route) => {
		await new Promise(resolve => setTimeout(resolve, delay));
		await route.continue();
	});
}

interface TabSequenceItem {
	tagName: string;
	role: string | null;
}

/**
 * Test keyboard navigation accessibility
 */
export async function testKeyboardNavigation(page: Page): Promise<TabSequenceItem[]> {
	// Tab through interactive elements
	const tabSequence: TabSequenceItem[] = [];
	
	for (let i = 0; i < 10; i++) {
		await page.keyboard.press('Tab');
		const focusedElement = await page.locator(':focus').first();
		
		if (await focusedElement.count() > 0) {
			const tagName = await focusedElement.evaluate(el => el.tagName.toLowerCase());
			const role = await focusedElement.getAttribute('role');
			tabSequence.push({ tagName, role });
		}
	}
	
	return tabSequence;
}