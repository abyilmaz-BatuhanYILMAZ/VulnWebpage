import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: './tests',
  testMatch: '**/*.spec.mjs',
  timeout: 30_000,
  expect: { timeout: 10_000 },
  retries: 0,
  workers: 1,
  reporter: [['list']],
  use: {
    baseURL: process.env.LAB_BASE_URL || 'http://127.0.0.1:8080',
    trace: 'retain-on-failure',
    ...devices['Desktop Chrome']
  }
});
