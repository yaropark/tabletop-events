import { Routes } from '@angular/router';
import { EventPage } from './event-page';
import { NewEventPage } from './new-event-page';
import { RegisterPage } from './register-page';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'calendar' },
  // Lazy so players opening a registration link on a phone don't download FullCalendar.
  {
    path: 'calendar',
    loadComponent: () => import('./calendar-page').then((m) => m.CalendarPage),
    title: 'Calendar',
  },
  { path: 'events/new', component: NewEventPage, title: 'New event' },
  { path: 'events/:id', component: EventPage, title: 'Event' },
  { path: 'register/:id', component: RegisterPage, title: 'Register' },
  { path: '**', redirectTo: 'calendar' },
];
