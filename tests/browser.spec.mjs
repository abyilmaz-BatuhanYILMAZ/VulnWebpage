import { test, expect } from '@playwright/test';

const injected = '<img src=x onerror="window.__labXss = 1">';
const stored = '<img src=x onerror="window.__storedXss = 1">';
const query = value => encodeURIComponent(value);

test('reflected XSS executes only in the unsafe sink with CSP disabled', async ({ page }) => {
  await page.goto('/?csp=off&reflectedInput=' + query(injected));
  await expect(page.getByTestId('unsafe-reflected-output').locator('img')).toHaveCount(1);
  await expect(page.getByTestId('safe-reflected-output').locator('img')).toHaveCount(0);
  await expect(page.getByTestId('safe-reflected-output')).toContainText(injected);
  await expect.poll(() => page.evaluate(() => window.__labXss)).toBe(1);
});

test('enforced CSP blocks inline event handlers without disabling the DOM demo', async ({ page }) => {
  const response = await page.goto('/?csp=strict&reflectedInput=' +
    query(injected) + '&domInput=' + query(injected));
  expect(response.headers()['content-security-policy']).toContain("script-src 'self'");
  await expect(page.getByTestId('unsafe-reflected-output').locator('img')).toHaveCount(1);
  await expect(page.locator('#domUnsafeTarget img')).toHaveCount(1);
  await expect(page.locator('#domSafeTarget img')).toHaveCount(0);
  await expect(page.locator('#domSafeTarget')).toContainText(injected);
  await expect(page.locator('#runDomDemo')).toBeEnabled();
  await page.waitForTimeout(200);
  expect(await page.evaluate(() => window.__labXss)).toBeUndefined();
});

test('DOM XSS contrasts innerHTML with textContent', async ({ page }) => {
  await page.goto('/?csp=off&domInput=' + query(injected));
  await expect(page.locator('#domUnsafeTarget img')).toHaveCount(1);
  await expect(page.locator('#domSafeTarget img')).toHaveCount(0);
  await expect(page.locator('#domSafeTarget')).toContainText(injected);
  await expect.poll(() => page.evaluate(() => window.__labXss)).toBe(1);
});

test('stored XSS persists while encoded output remains inert', async ({ page }) => {
  await page.goto('/?csp=off');
  await page.locator('#StoredTitle').fill('Browser regression test');
  await page.locator('#StoredContent').fill(stored);
  await page.getByRole('button', { name: 'Save Stored Input' }).click();
  await expect(page.getByTestId('unsafe-stored-output').locator('img').first()).toHaveCount(1);
  await expect(page.getByTestId('safe-stored-output').locator('img')).toHaveCount(0);
  await expect(page.getByTestId('safe-stored-output').first()).toContainText(stored);
  await expect.poll(() => page.evaluate(() => window.__storedXss)).toBe(1);
  await page.reload();
  await expect(page.getByTestId('unsafe-stored-output').locator('img').first()).toHaveCount(1);
});
