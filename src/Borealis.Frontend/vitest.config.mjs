import { defineConfig } from 'vitest/config';

export default defineConfig({
  test: {
    setupFiles: ['./scripts/test-setup.mjs'],
    environment: 'jsdom',
    reporters: ['dot'],
    coverage: {
      reporter: ['text', 'cobertura', 'html']
    }
  }
});
