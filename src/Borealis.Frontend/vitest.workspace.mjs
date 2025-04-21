import { defineConfig } from 'vitest/config';

export default defineConfig([
  {
    test: {
      name: 'jsdom',
      environment: 'jsdom',
      exclude: ['**/node_modules/**', './scripts/**/*.browser.spec.mjs'],
      setupFiles: ['./scripts/test-setup.mjs'],
    },
    extends: true
  },
  {
    test: {
      name: 'browser',
      browser: {
        provider: 'playwright',
        headless: true,
        enabled: true,
        instances: [
          { browser: 'chromium' },
        ]
      },
      extends: false,
      include: ['**/*.browser.spec.mjs']
    }
  }
]);
