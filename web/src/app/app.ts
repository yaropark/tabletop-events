import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    <header class="topbar">
      <a routerLink="/" class="brand">Tabletop Events</a>
      <nav>
        <a routerLink="/calendar" routerLinkActive="active">Calendar</a>
        <a routerLink="/events/new" routerLinkActive="active">New event</a>
      </nav>
    </header>
    <main><router-outlet /></main>
  `,
})
export class App {}
