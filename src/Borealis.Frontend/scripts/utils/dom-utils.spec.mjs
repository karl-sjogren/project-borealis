import { describe, test, expect } from 'vitest';
import { firstAnscestorOrDefault } from './dom-utils.mjs';

describe('dom-utils', () => {
  describe('firstAnscestorOrDefault', () => {

    test('should find matching parent', () => {
      const container = document.createElement('div');
      container.innerHTML = `
        <div class="root">
          <div class="parent parent3">
            <div class="parent parent2">
              <div class="parent parent1">
              </div>
            </div>
          </div>
        </div>
      `;

      const childElement = container.querySelector('.parent1');

      const foundElement = firstAnscestorOrDefault(childElement, element => element.classList.contains('parent3'));
      expect(foundElement.classList.contains('parent3')).toBe(true);
    });

    test('if includeSelf is true, should check initial element', () => {
      const container = document.createElement('div');
      container.innerHTML = `
        <div class="root">
          <div class="parent parent3">
            <div class="parent parent2">
              <div class="parent parent1">
              </div>
            </div>
          </div>
        </div>
      `;

      const childElement = container.querySelector('.parent1');

      const foundElement = firstAnscestorOrDefault(childElement, element => element.classList.contains('parent'), true);
      expect(foundElement.classList.contains('parent1')).toBe(true);
    });

    test('should find first matching parent', () => {
      const container = document.createElement('div');
      container.innerHTML = `
        <div class="root">
          <div class="parent parent3">
            <div class="parent parent2">
              <div class="parent parent1">
              </div>
            </div>
          </div>
        </div>
      `;

      const childElement = container.querySelector('.parent1');

      const foundElement = firstAnscestorOrDefault(childElement, element => element.classList.contains('parent'));
      expect(foundElement.classList.contains('parent2')).toBe(true);
    });

    test('return null if not matching parent', () => {
      const container = document.createElement('div');
      container.innerHTML = `
        <div class="root">
          <div class="parent parent3">
            <div class="parent parent2">
              <div class="parent parent1">
              </div>
            </div>
          </div>
        </div>
      `;

      const childElement = container.querySelector('.parent1');

      const foundElement = firstAnscestorOrDefault(childElement, element => element.classList.contains('parent4'));
      expect(foundElement).toBe(null);
    });

  });
});
