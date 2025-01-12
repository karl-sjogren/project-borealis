import '../styles/main.scss';
import 'bootstrap';
import { Popover } from 'bootstrap';

const popoverTriggerList = document.querySelectorAll('[data-bs-toggle="popover"]');
[...popoverTriggerList].map(popoverTriggerEl => new Popover(popoverTriggerEl));
