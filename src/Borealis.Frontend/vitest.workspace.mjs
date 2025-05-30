import { defineWorkspace } from 'vitest/config';

/** @type {import('vitest/node').UserWorkspaceConfig} */
export default defineWorkspace([
  {
    test: {
      name: 'jsdom',
      environment: 'jsdom',
      exclude: ['**/node_modules/**', './scripts/**/*.browser.spec.mjs'],
      setupFiles: ['./scripts/test-setup.mjs'],
    }
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
      include: ['./scripts/**/*.browser.spec.mjs']
    }
  }
]);
