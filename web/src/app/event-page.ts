import { DatePipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { catchError, forkJoin, map, of, switchMap, timer } from 'rxjs';
import { Api } from './api';

@Component({
  selector: 'app-event-page',
  imports: [DatePipe, RouterLink],
  template: `
    @if (data(); as d) {
      @if (d.event; as e) {
        <div class="page-head">
          <p class="tag" [style.background]="e.color">{{ e.gameName }} · {{ e.format }}</p>
          <h1>{{ e.name }}</h1>
          <p>
            {{ e.startLocal | date: 'EEEE, MMMM d, y' }} · {{ e.startLocal | date: 'h:mm a' }} –
            {{ e.endLocal | date: 'h:mm a' }} <span class="muted">({{ e.timeZone }})</span>
          </p>
          <p>{{ e.location }}</p>
        </div>

        <div class="grid">
          <section class="card">
            <h2>Players</h2>
            <p class="big">{{ e.registeredCount }} / {{ e.capacity }}</p>
            <div class="meter"><div [style.width.%]="(100 * e.registeredCount) / e.capacity"></div></div>
            <p>
              @if (e.seatsLeft === 0) {
                <strong>Event is full.</strong>
              } @else {
                {{ e.seatsLeft }} seat{{ e.seatsLeft === 1 ? '' : 's' }} left.
              }
              @if (e.registeredCount >= e.minPlayers) {
                Enough players to start.
              } @else {
                Needs {{ e.minPlayers - e.registeredCount }} more to start (minimum {{ e.minPlayers }}).
              }
            </p>
            <a class="button secondary" [href]="api.icsUrl(e.id)" download>Add to calendar (.ics)</a>

            <h3>Registered</h3>
            <ol class="registrants">
              @for (r of d.registrants; track $index) {
                <li>{{ r.name }}</li>
              } @empty {
                <li class="muted">No one yet.</li>
              }
            </ol>
          </section>

          <section class="card center">
            <h2>Register</h2>
            <img class="qr" [src]="api.qrUrl(e.id)" alt="QR code for the registration link" />
            <p><a [routerLink]="['/register', e.id]">{{ e.registrationUrl }}</a></p>
            <p class="muted">Players scan this code to sign up.</p>
          </section>
        </div>
      } @else {
        <p class="alert error">Event not found.</p>
      }
    }
  `,
})
export class EventPage {
  protected readonly api = inject(Api);

  // Refresh every few seconds so the organizer sees registrations arrive from players' phones.
  protected readonly data = toSignal(
    inject(ActivatedRoute).paramMap.pipe(
      map((p) => Number(p.get('id'))),
      switchMap((id) =>
        timer(0, 5000).pipe(
          switchMap(() =>
            forkJoin({ event: this.api.event(id), registrants: this.api.registrants(id) }).pipe(
              catchError(() => of({ event: null, registrants: [] })),
            ),
          ),
        ),
      ),
    ),
  );
}
