import { test, expect } from '@playwright/test';

test('overview introduces all six modules and their local exercise links', async ({ page }) => {
  const response = await page.goto('/labs/');
  expect(response.status()).toBe(200);
  await expect(page.getByRole('heading', { name: 'Application security exercises' })).toBeVisible();
  await expect(page.getByTestId('lab-card')).toHaveCount(6);
  for (const path of [
    '/?csp=off',
    '/labs/authorization',
    '/labs/sql-injection',
    '/labs/ssrf',
    '/labs/path-traversal',
    '/labs/mass-assignment'
  ]) {
    await expect(page.locator('a[href="' + path + '"]').first()).toBeVisible();
  }
});

test('practice checklist persists locally and can be cleared', async ({ page }) => {
  await page.goto('/labs/');
  const xss = page.locator('[data-progress-check="xss"]');
  const path = page.locator('[data-progress-check="path-traversal"]');
  await xss.check();
  await path.check();
  await expect(page.locator('[data-progress-label]')).toContainText('2 / 6 modules');
  await page.reload();
  await expect(xss).toBeChecked();
  await expect(path).toBeChecked();
  await expect(page.locator('[data-progress-bar]')).toHaveAttribute('aria-valuenow', '2');
  await page.getByRole('button', { name: 'Reset progress' }).click();
  await expect(xss).not.toBeChecked();
  await expect(path).not.toBeChecked();
  await expect(page.locator('[data-progress-label]')).toContainText('0 / 6 modules');
  await page.reload();
  await expect(xss).not.toBeChecked();
});

test('virtual path traversal exposes an internal fixture in unsafe mode', async ({ page }) => {
  await page.goto('/labs/path-traversal?path=..%2Finternal%2Fconfig.txt&mode=unsafe');
  await expect(page.getByTestId('path-content')).toContainText('SIMULATED_PRIVATE_CONFIGURATION');
  await expect(page.getByTestId('path-normalized')).toContainText('/internal/config.txt');
});

test('canonical path confinement denies the same traversal input', async ({ page }) => {
  await page.goto('/labs/path-traversal?path=..%2Finternal%2Fconfig.txt&mode=safe');
  await expect(page.getByTestId('path-outcome')).toContainText('Blocked');
  await expect(page.getByTestId('path-content')).toHaveCount(0);
});

test('safe virtual file reads work for a legitimate public fixture', async ({ page }) => {
  await page.goto('/labs/path-traversal?path=readme.txt&mode=safe');
  await expect(page.getByTestId('path-content')).toContainText('PUBLIC_README_FIXTURE');
  await expect(page.getByTestId('path-normalized')).toContainText('/public/readme.txt');
});

test('untrusted profile role flag is assigned only by the unsafe binding', async ({ page }) => {
  await page.goto('/labs/mass-assignment');
  await page.locator('#displayName').fill('Student');
  await page.locator('#isAdmin').check();
  await page.locator('#mode').selectOption('unsafe');
  await page.getByRole('button', { name: 'Submit simulated update' }).click();
  await expect(page.getByTestId('mass-outcome')).toContainText('untrusted IsAdmin');
  await expect(page.getByTestId('mass-profile').locator('code')).toHaveText('true');
});

test('field allowlist ignores the extra role flag', async ({ page }) => {
  await page.goto('/labs/mass-assignment');
  await page.locator('#displayName').fill('Student');
  await page.locator('#isAdmin').check();
  await page.locator('#mode').selectOption('safe');
  await page.getByRole('button', { name: 'Submit simulated update' }).click();
  await expect(page.getByTestId('mass-outcome')).toContainText('ignored by the allowlist');
  await expect(page.getByTestId('mass-profile').locator('code')).toHaveText('false');
});

test('mass assignment POST requires a CSRF token', async ({ request }) => {
  const response = await request.post('/labs/mass-assignment', {
    form: { displayName: 'Student', isAdmin: 'true', mode: 'unsafe' }
  });
  expect(response.status()).toBe(400);
});

test('additional exercise pages use default restrictive CSP and no-store', async ({ page }) => {
  for (const path of ['/labs/', '/labs/path-traversal', '/labs/mass-assignment']) {
    const response = await page.goto(path);
    expect(response.status()).toBe(200);
    expect(response.headers()['content-security-policy']).toContain("script-src 'self'");
    expect(response.headers()['cache-control']).toContain('no-store');
  }
});
