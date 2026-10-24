import { Routes } from '@angular/router';
import { EventCreateComponent } from './components/event-create/event-create.component';
import { EventDetailsComponent } from './components/event-details/event-details.component';
import { EventRegisterComponent } from './components/event-register/event-register.component';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'calendar' },
  // Lazy so players opening a registration link on a phone don't download FullCalendar.
  {
    path: 'calendar',
    loadComponent: () => import('./components/calendar/calendar.component').then((m) => m.CalendarComponent),
    title: 'Calendar',
  },
  { path: 'events/new', component: EventCreateComponent, title: 'New event' },
  { path: 'events/:id', component: EventDetailsComponent, title: 'Event' },
  { path: 'register/:id', component: EventRegisterComponent, title: 'Register' },
  { path: '**', redirectTo: 'calendar' },
];
