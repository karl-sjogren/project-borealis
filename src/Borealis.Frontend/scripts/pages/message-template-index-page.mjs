import { Dropdown } from 'bootstrap';
import { firstAnscestorOrDefault } from '../utils/dom-utils.mjs';

/**
 * @param {Element} rootElement
 */
export default (rootElement = document.body) => {
  const pageElement = rootElement?.querySelector('#message-template-index-page');

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

    if(!button.classList.contains('delete-message-template-button')) {
      return;
    }

    if(!confirm('Are you sure you want to delete this message template?')) {
      return;
    }

    event.stopPropagation();
    event.preventDefault();

    const row = firstAnscestorOrDefault(button, element => element.classList.contains('message-template-row'));
    const messageTemplateId = row.dataset.messageTemplateId;

    await deleteMessageTemplate(messageTemplateId);

    row.remove();
  });
};

/**
 * Removes a message template from the system
 * @param {string} messageTemplateId The message template ID
 */
const deleteMessageTemplate = async(messageTemplateId) => {
  const url = `/api/messages/${messageTemplateId}`;

  const response = await fetch(url, { method: 'DELETE' });

  if(!response.ok) {
    console.error(`Failed to delete message template: ${response.status} ${response.statusText}`);
    return null;
  }
};
