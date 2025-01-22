import giftCodeIndexPage from './gift-code-index-page.mjs';
import messageTemplateIndexPage from './message-template-index-page.mjs';
import initPlayersIndexPage from './players-index-page.mjs';
import inutUsersIndexPage from './users-index-page.mjs';

/**
 * Init all scripts for specific pages
 */
export default function() {
  giftCodeIndexPage();
  messageTemplateIndexPage();
  initPlayersIndexPage();
  inutUsersIndexPage();
}
