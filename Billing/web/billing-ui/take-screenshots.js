import { chromium } from 'playwright';
import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';

const __dirname = path.dirname(fileURLToPath(import.meta.url));

async function takeScreenshots() {
    // Create screenshots directory if it doesn't exist
    const screenshotsDir = path.join(__dirname, 'screenshots');
    if (!fs.existsSync(screenshotsDir)) {
        fs.mkdirSync(screenshotsDir);
    }

    const browser = await chromium.launch({ headless: true });
    const page = await browser.newPage();
    await page.setViewportSize({ width: 1280, height: 720 });

    try {
        // Screenshot 1: Home page
        console.log('Taking screenshot of home page...');
        await page.goto('http://localhost:3000');
        await page.waitForLoadState('networkidle');
        await page.waitForTimeout(2000); // Extra wait for content to load
        await page.screenshot({ path: 'screenshots/01-home-page.png', fullPage: true });

        // Screenshot 2: Navigate to cashiers
        console.log('Taking screenshot of cashiers page...');
        // Try different selectors for the button
        try {
            await page.getByRole('button', { name: 'Manage Cashiers' }).click({ timeout: 10000 });
        } catch (e) {
            // Try alternative selector
            await page.locator('button:has-text("Manage Cashiers")').click({ timeout: 10000 });
        }
        await page.waitForURL('/cashiers', { timeout: 10000 });
        await page.waitForLoadState('networkidle');
        await page.waitForTimeout(2000);
        await page.screenshot({ path: 'screenshots/02-cashiers-list.png', fullPage: true });

        // Screenshot 3: Create cashier form
        console.log('Taking screenshot of create cashier form...');
        await page.getByRole('button', { name: 'Add Cashier' }).click();
        await page.waitForURL('/cashiers/create');
        await page.waitForLoadState('networkidle');
        await page.screenshot({ path: 'screenshots/03-create-cashier-form.png', fullPage: true });

        // Screenshot 4: Fill form partially
        console.log('Taking screenshot of filled form...');
        await page.getByLabel('Name *').fill('John Smith');
        await page.getByLabel('Email *').fill('john.smith@example.com');
        await page.getByPlaceholder('Add currency (e.g., USD)').fill('EUR');
        await page.getByRole('button').filter({ hasText: '+' }).click();
        await page.screenshot({ path: 'screenshots/04-form-filled.png', fullPage: true });

        // Screenshot 5: Go back to cashiers and view details
        console.log('Taking screenshot of cashier details...');
        await page.getByRole('button').first().click(); // Back button
        await page.waitForURL('/cashiers');
        await page.waitForLoadState('networkidle');
        
        // Click on first cashier if it exists
        const cashierCard = page.getByText('Test Cashier').first();
        if (await cashierCard.isVisible()) {
            await cashierCard.click();
            await page.waitForLoadState('networkidle');
            await page.screenshot({ path: 'screenshots/05-cashier-details.png', fullPage: true });
        }

        console.log('Screenshots saved to ./screenshots/');
        console.log('Files created:');
        console.log('- 01-home-page.png');
        console.log('- 02-cashiers-list.png');
        console.log('- 03-create-cashier-form.png');
        console.log('- 04-form-filled.png');
        console.log('- 05-cashier-details.png');

    } catch (error) {
        console.error('Error taking screenshots:', error);
    } finally {
        await browser.close();
    }
}

takeScreenshots();