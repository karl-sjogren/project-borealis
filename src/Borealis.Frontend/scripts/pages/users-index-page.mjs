import { Dropdown } from 'bootstrap';
import { firstAncestorOrDefault } from '../utils/dom-utils.mjs';

/**
 * @param {Element} rootElement
 */
export default (rootElement = document.body) => {
  const pageElement = rootElement?.querySelector('#users-index-page');

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

    /**
     * @type {HTMLButtonElement}
     */
    const button = event.target;

    if(!button.classList.contains('delete-user-button')) {
      return;
    }

    if(!confirm('Are you sure you want to delete this user?')) {
      return;
    }

    event.stopPropagation();
    event.preventDefault();

    const row = firstAncestorOrDefault(button, element => element.classList.contains('user-row'));
    const userId = row.dataset.userId;

    await deleteUser(userId);

    row.remove();
  });
};

/**
 * Removes a user from the system
 * @param {string} userId The user ID
 */
const deleteUser = async(userId) => {
  const url = `/api/users/${userId}`;

  const response = await fetch(url, { method: 'DELETE' });

  if(!response.ok) {
    console.error(`Failed to delete user: ${response.status} ${response.statusText}`);
    return null;
  }
};
