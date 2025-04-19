import '../styles/main.scss';
import 'bootstrap';
import { Popover } from 'bootstrap';
import './pages/init.mjs';
import './elements/init.mjs';

const popoverTriggerList = document.querySelectorAll('[data-bs-toggle="popover"]');
[...popoverTriggerList].map(popoverTriggerEl => new Popover(popoverTriggerEl));

const copyButtons = document.querySelectorAll('.button-copy-data');
[...copyButtons].map(copyButton => {
  copyButton.addEventListener('click', e => {
    const copyText = e.currentTarget.dataset.copyData;
    if(!copyText) {
      return;
    }

    navigator
      .clipboard
      .writeText(copyText)
      .then(null, () => {
        alert('Failed to copy to clipboard');
      });
  });
});
