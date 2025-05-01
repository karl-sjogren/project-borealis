import { page } from '@vitest/browser/context'
import { afterEach, beforeEach, describe, expect, it } from 'vitest';
import Pretender from 'pretender';

import './players-table-element.mjs'
import { waitFor } from '@testing-library/dom';

describe('players-table-element', async () => {
  let server;

  beforeEach(() => {
    const players = [
      {
        externalId: 1,
        name: 'Player 1',
        state: 123,
        furnaceLevelString: 'FC2'
      }
    ];

    const element = document.createElement('players-table');
    element.setAttribute('players', JSON.stringify(players));

    document.body.appendChild(element);

    server = new Pretender(function() {
      this.put('/api/player', request => [404, {}, '']);
    });
  });

  afterEach(() => {
    document.body.innerHTML = '';

    server.shutdown();
  });

  it('should render table within a table-responsive container', async () => {
    const table = document.querySelector('.players-table');

    expect(table).toBeTruthy();

    const parent = table.parentElement;

    expect(parent).toBeTruthy();
    expect(parent.tagName).toBe('DIV');
    expect(parent.classList.contains('table-responsive')).toBeTruthy();
  });

  it('should render a table with the correct number of columns', async () => {
    const table = document.querySelector('.players-table');

    expect(table).toBeTruthy();

    const columns = [...table.querySelectorAll('th')];
    expect(columns.length).toBe(6);

    const columnHeaders = columns.map(column => column.textContent.trim());
    expect(columnHeaders).toEqual([
      'Player ID',
      'Level',
      'State',
      'In alliance',
      'Name',
      ''
    ]);
  });

  it('should render a table row for each player', async () => {
    const rows = [...document.querySelectorAll('.players-table tbody tr')];
    expect(rows.length).toBe(1);

    const playerId = rows[0].querySelector('td:nth-child(1)').textContent.trim();
    expect(playerId).toBe('1');

    const level = rows[0].querySelector('td:nth-child(2)').textContent.trim();
    expect(level).toBe('FC2');

    const state = rows[0].querySelector('td:nth-child(3)').textContent.trim();
    expect(state).toBe('123');
  });
})
