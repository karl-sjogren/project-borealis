import { defineConfig } from 'vitest/config';

/** @type {import('vitest').ViteUserConfig} */
export default defineConfig({
  test: {
    reporters: ['dot'],
    coverage: {
      reporter: ['text', 'cobertura', 'html'],
      include: [
        'scripts/**/*.mjs'
      ]
    }
  }
});
