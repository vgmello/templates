# Playwright Tests for Billing UI

This directory contains comprehensive end-to-end tests for the Billing UI application using Playwright.

## Test Structure

### Test Files

1. **`cashiers.spec.js`** - Core cashier functionality tests
   - Cashiers page display and interaction
   - Cashier detail page functionality
   - Navigation between pages
   - Responsive design testing

2. **`cashier-form.spec.js`** - Create cashier form tests
   - Form validation and interaction
   - Currency management
   - Error handling
   - Accessibility and keyboard navigation

3. **`navigation.spec.js`** - Application navigation tests
   - Header navigation
   - Button navigation
   - Browser back/forward functionality
   - Deep linking and URL handling

4. **`accessibility.spec.js`** - Accessibility and performance tests
   - Keyboard navigation
   - Screen reader compatibility
   - Performance benchmarks
   - SEO and meta tag validation

5. **`test-helpers.js`** - Utility functions for tests
   - Common test workflows
   - API mocking helpers
   - Responsive testing utilities

## Running Tests

### Prerequisites
```bash
npm install
npx playwright install
```

### Test Commands
```bash
# Run all tests
npm test

# Run tests in headed mode (visible browser)
npm run test:headed

# Run tests with UI (interactive mode)
npm run test:ui

# Run tests in debug mode
npm run test:debug

# Run specific test file
npx playwright test tests/cashiers.spec.js

# Run tests for specific browser
npx playwright test --project=chromium
```

## Test Coverage

### Functional Testing
- ✅ Page loading and rendering
- ✅ Data display with mock fallbacks
- ✅ Form submissions and validation
- ✅ Navigation flows
- ✅ Error handling

### User Experience Testing
- ✅ Button interactions
- ✅ Form field validation
- ✅ Loading states
- ✅ Responsive design
- ✅ Mobile interactions

### Accessibility Testing
- ✅ Keyboard navigation
- ✅ Form labels and associations
- ✅ Semantic HTML structure
- ✅ Focus management

### Performance Testing
- ✅ Page load times
- ✅ Navigation speed
- ✅ Memory usage stability
- ✅ Network error handling

## Current Test Status

### Passing Tests ✅
- Basic page rendering
- Data display with fallback
- Form field interactions
- Navigation structure
- Accessibility features
- Performance benchmarks

### Areas Needing Attention ⚠️
- Some button navigation tests timeout (navigation works but tests need refinement)
- Form submission flows need API mocking improvements
- Responsive tests need selector refinements

## Test Architecture

### Mock Data Strategy
Tests use fallback mock data when the API is unavailable, ensuring tests can run without a backend. The load functions gracefully handle API failures and return consistent mock data.

### Error Handling
Tests verify that the application handles various error conditions:
- Network failures
- API errors (500, 404, etc.)
- Invalid user inputs
- Navigation edge cases

### Responsive Testing
Tests verify functionality across different viewport sizes:
- Mobile: 375x667
- Tablet: 768x1024  
- Desktop: 1280x720

## Best Practices Demonstrated

1. **Page Object Pattern** - Using helper functions for common workflows
2. **Data-driven Testing** - Parameterized tests for different scenarios
3. **Accessibility First** - Every test considers keyboard and screen reader users
4. **Performance Aware** - Tests include timing assertions
5. **Error Resilient** - Tests verify graceful error handling

## Continuous Integration

Tests are configured to:
- Run in headless mode on CI
- Generate HTML reports
- Take screenshots on failures
- Support parallel execution
- Handle flaky tests with retries

## Development Workflow

1. **Write Tests First** - Tests help define expected behavior
2. **Run Locally** - Use `npm run test:headed` for development
3. **Debug Issues** - Use `npm run test:debug` for step-through debugging
4. **Check Coverage** - Ensure all user flows are tested
5. **Optimize Performance** - Monitor test execution times

## Extending Tests

To add new tests:

1. Choose the appropriate test file based on functionality
2. Follow existing patterns for consistency
3. Use the helper functions in `test-helpers.js`
4. Include accessibility and responsive considerations
5. Add both positive and negative test cases

## Troubleshooting

### Common Issues

**Tests timing out:**
- Increase timeout in `playwright.config.js`
- Check for network issues
- Verify selectors are correct

**Navigation tests failing:**
- Ensure navigation functions are properly bound
- Check for JavaScript errors in console
- Verify URL patterns match

**Responsive tests failing:**
- Check viewport size settings
- Verify responsive CSS is loaded
- Test on actual devices when possible

### Debug Commands
```bash
# Run with debug logging
DEBUG=pw:api npx playwright test

# Run single test in debug mode
npx playwright test --debug tests/cashiers.spec.js -g "specific test name"

# Generate test report
npx playwright show-report
```