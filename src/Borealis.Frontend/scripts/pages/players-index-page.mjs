import { Dropdown } from 'bootstrap';
import { firstAnscestorOrDefault } from '../utils/dom-utils.mjs';

/**
 * @param {Element} rootElement
 */
export default (rootElement = document.body) => {
  const pageElement = rootElement?.querySelector('#players-index-page');

  if(!pageElement) {
    return;
  }

  initTableButtons(pageElement);
};

const initTableButtons = pageElement => {
  pageElement.addEventListener('click', async event => {
    if(!(event.target instanceof HTMLButtonElement)) {
      return;
    }

    event.stopPropagation();
    event.preventDefault();

    /**
     * @type {HTMLButtonElement}
     */
    const button = event.target;

    const row = firstAnscestorOrDefault(button, element => element.classList.contains('player-row'));

    if(!row) {
      return;
    }

    if(button.classList.contains('add-to-alliance-button') || button.classList.contains('remove-from-alliance-button')) {
      await allianceButtonClickHandler(row, button);
      closeDropdown(row, event);
    }

    if(button.classList.contains('mute-button') || button.classList.contains('unmute-button')) {
      await muteButtonClickHandler(row, button);
      closeDropdown(row, event);
    }

    if(button.classList.contains('delete-player-button')) {
      await deletePlayerButtonClickHandler(row, button);
      closeDropdown(row, event);
    }
  });
};

const closeDropdown = (row, event) => {
  const dropdownToggle = new Dropdown(row.querySelector('.dropdown-toggle'));
  dropdownToggle.hide();
}

const deleteButtonClickHandler = async(row, button) => {
  if(!confirm('Are you sure you want to delete this player?')) {
    return;
  }

  const playerId = row.dataset.playerId;

  await deletePlayer(playerId);

  row.remove();
};

const allianceButtonClickHandler = async(row, button) => {
  const isAddButton = button.classList.contains('add-to-alliance-button');

  const playerId = row.dataset.playerId;

  await updateAllianceStatus(playerId, isAddButton);

  const inAllianceCell = row.querySelector('.in-alliance');

  inAllianceCell.textContent = isAddButton ? 'Yes' : 'No';

  row.classList.toggle('is-in-alliance', isAddButton);
  row.classList.toggle('is-not-in-alliance', !isAddButton);
};

const muteButtonClickHandler = async(row, button) => {
  const isMuteButton = button.classList.contains('mute-button');

  const playerId = row.dataset.playerId;

  await updateMuteSetting(playerId, isMuteButton);

  const isMutedCell = row.querySelector('.muted');

  isMutedCell.textContent = isMuteButton ? 'Yes' : 'No';

  row.classList.toggle('is-muted', isMuteButton);
  row.classList.toggle('is-not-muted', !isMuteButton);
}

/**
 * Adds or remove a player from the alliance
 * @param {string} playerId The player ID
 * @param {boolean} addToAlliance True to add to alliance, false to remove from alliance
 */
const updateAllianceStatus = async(playerId, addToAlliance) => {
  const url = `/api/players/${playerId}/${addToAlliance ? 'add-to-alliance' : 'remove-from-alliance'}`;

  const response = await fetch(url, { method: 'PUT' });

  if(!response.ok) {
    console.error(`Failed to change alliance status of player: ${response.status} ${response.statusText}`);
    return null;
  }
};

/**
 *
 * @param {string} playerId The player ID
 * @param {boolean} mute True to mute, false to unmute
 * @returns
 */
const updateMuteSetting = async(playerId, mute) => {
  const url = `/api/players/${playerId}/${mute ? 'mute' : 'unmute'}`;

  const response = await fetch(url, { method: 'PUT' });

  if(!response.ok) {
    console.error(`Failed to update mute setting of player: ${response.status} ${response.statusText}`);
    return null;
  }
};

/**
 * Removes a player from the system
 * @param {string} playerId The player ID
 */
const deletePlayer = async(playerId) => {
  const url = `/api/players/${playerId}`;

  const response = await fetch(url, { method: 'DELETE' });

  if(!response.ok) {
    console.error(`Failed to delete player: ${response.status} ${response.statusText}`);
    return null;
  }
};
