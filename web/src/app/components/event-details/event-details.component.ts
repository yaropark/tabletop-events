import { DatePipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { catchError, forkJoin, map, of, switchMap, timer } from 'rxjs';
import { EventService } from '../../services/event.service';
import { RegistrationService } from '../../services/registration.service';

@Component({
  selector: 'app-event-details',
  imports: [DatePipe, RouterLink],
  templateUrl: './event-details.component.html',
  styleUrl: './event-details.component.scss',
})
export class EventDetailsComponent {
  protected readonly eventService = inject(EventService);
  private readonly registrationService = inject(RegistrationService);

  // Illustrative only: the app can't see the host's LAN IP from inside Docker, so this isn't a link.
  protected readonly exampleLanUrl = `http://192.168.1.20${location.port ? ':' + location.port : ''}`;

  protected isLocalhost(url: string) {
    return ['localhost', '127.0.0.1', '[::1]'].includes(new URL(url).hostname);
  }

  // Refresh every few seconds so the organizer sees registrations arrive from players' phones.
  protected readonly data = toSignal(
    inject(ActivatedRoute).paramMap.pipe(
      map((p) => Number(p.get('id'))),
      switchMap((id) =>
        timer(0, 5000).pipe(
          switchMap(() =>
            forkJoin({
              event: this.eventService.getEvent(id),
              registrants: this.registrationService.getRegistrants(id),
            }).pipe(catchError(() => of({ event: null, registrants: [] }))),
          ),
        ),
      ),
    ),
  );
}
