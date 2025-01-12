import '../styles/main.scss';
import 'bootstrap';
import { Popover } from 'bootstrap';
import initPages from './pages/init.mjs';

const popoverTriggerList = document.querySelectorAll('[data-bs-toggle="popover"]');
[...popoverTriggerList].map(popoverTriggerEl => new Popover(popoverTriggerEl));

initPages();
