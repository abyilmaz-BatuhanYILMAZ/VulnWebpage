import { test, expect } from '@playwright/test';

test('lab navigation exposes all three training topics', async ({ page }) => {
  await page.goto('/labs/');
  await expect(page.getByRole('heading', { name: 'Application security exercises' })).toBeVisible();
  for (const path of ['/labs/authorization', '/labs/sql-injection', '/labs/ssrf']) {
    await expect(page.locator('a[href="' + path + '"]').first()).toBeVisible();
  }
});

test('IDOR unsafe read exposes a different actor synthetic document', async ({ page }) => {
  await page.goto('/labs/authorization?actor=alice&id=202&mode=unsafe');
  await expect(page.getByTestId('authorization-outcome')).toContainText('Document returned.');
  await expect(page.getByTestId('authorization-document')).toContainText('BOB_PRIVATE_DEMO_DOCUMENT');
});

test('IDOR safe read denies the same cross-owner document', async ({ page }) => {
  await page.goto('/labs/authorization?actor=alice&id=202&mode=safe');
  await expect(page.getByTestId('authorization-outcome')).toContainText('Access denied');
  await expect(page.getByTestId('authorization-document')).toHaveCount(0);
  await page.goto('/labs/authorization?actor=bob&id=202&mode=safe');
  await expect(page.getByTestId('authorization-document')).toContainText('BOB_PRIVATE_DEMO_DOCUMENT');
});

test('SQL injection changes unsafe read-only query results', async ({ page }) => {
  await page.goto("/labs/sql-injection?mode=unsafe&term=" + encodeURIComponent("' OR 1=1--"));
  await expect(page.getByTestId('sql-outcome')).toContainText('Matched 4 inventory records.');
  await expect(page.getByTestId('sql-item')).toHaveCount(4);
});

test('parameterized SQL treats the injection input as literal text', async ({ page }) => {
  await page.goto("/labs/sql-injection?mode=safe&term=" + encodeURIComponent("' OR 1=1--"));
  await expect(page.getByTestId('sql-outcome')).toContainText('Matched 0 inventory records.');
  await expect(page.getByTestId('sql-item')).toHaveCount(0);
  await page.goto('/labs/sql-injection?mode=safe&term=book');
  await expect(page.getByTestId('sql-item')).toHaveCount(1);
});

test('SQL injection exercise rejects statement separators', async ({ page }) => {
  await page.goto('/labs/sql-injection?mode=unsafe&term=' + encodeURIComponent("'; DROP TABLE Inventory;--"));
  await expect(page.getByTestId('sql-outcome')).toContainText('Input rejected');
  await expect(page.getByTestId('sql-item')).toHaveCount(0);
});

test('unsafe SSRF demonstration returns simulated internal fixture only', async ({ page }) => {
  await page.goto('/labs/ssrf?mode=unsafe&url=' +
    encodeURIComponent('http://internal.lab.invalid/admin'));
  await expect(page.getByTestId('ssrf-response')).toContainText('SIMULATED_INTERNAL_ADMIN_DEMO');
  await expect(page.getByTestId('ssrf-outcome')).toContainText('No network request was made');
});

test('safe SSRF demonstration rejects internal fixture but allows public one', async ({ page }) => {
  await page.goto('/labs/ssrf?mode=safe&url=' +
    encodeURIComponent('http://internal.lab.invalid/admin'));
  await expect(page.getByTestId('ssrf-outcome')).toContainText('Blocked');
  await expect(page.getByTestId('ssrf-response')).toHaveCount(0);
  await page.goto('/labs/ssrf?mode=safe&url=' +
    encodeURIComponent('https://public.lab.invalid/catalog'));
  await expect(page.getByTestId('ssrf-response')).toContainText('PUBLIC_CATALOG_DEMO');
});

test('new exercises enforce restrictive CSP response headers', async ({ page }) => {
  for (const path of ['/labs/', '/labs/authorization', '/labs/sql-injection', '/labs/ssrf']) {
    const response = await page.goto(path);
    expect(response.status()).toBe(200);
    expect(response.headers()['content-security-policy']).toContain("script-src 'self'");
    expect(response.headers()['cache-control']).toContain('no-store');
  }
});
