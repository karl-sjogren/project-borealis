import { Dropdown } from 'bootstrap';
import { firstAncestorOrDefault } from '../utils/dom-utils.mjs';

/**
 * @param {Element} rootElement
 */
export default (rootElement = document.body) => {
  const pageElement = rootElement?.querySelector('#gift-code-index-page');

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

    if(!button.classList.contains('redeem-again-button')) {
      return;
    }

    event.stopPropagation();
    event.preventDefault();

    const row = firstAncestorOrDefault(button, element => element.classList.contains('gift-code-row'));
    const giftCodeId = row.dataset.giftCodeId;

    await redeemGiftCode(giftCodeId);

    const dropdownToggle = new Dropdown(row.querySelector('.dropdown-toggle'));
    dropdownToggle.hide();
  });
};

/**
 * Enqueues a gift code for redemption
 * @param {string} giftCodeId The gift code ID
 */
const redeemGiftCode = async(giftCodeId) => {
  const url = `/api/gift-codes/${giftCodeId}/redeem`;

  const response = await fetch(url, { method: 'PUT' });

  if(!response.ok) {
    console.error(`Failed to enqueue gift code: ${response.status} ${response.statusText}`);
    return null;
  }
};
