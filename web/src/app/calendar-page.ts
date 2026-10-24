import { Component, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { Router } from '@angular/router';
import { FullCalendarModule } from '@fullcalendar/angular';
import { CalendarOptions, EventInput } from '@fullcalendar/core';
import dayGridPlugin from '@fullcalendar/daygrid';
import interactionPlugin from '@fullcalendar/interaction';
import listPlugin from '@fullcalendar/list';
import { firstValueFrom } from 'rxjs';
import { Api } from './api';
import { toIsoDate } from './dates';

@Component({
  selector: 'app-calendar-page',
  imports: [FullCalendarModule],
  template: `
    <div class="page-head">
      <h1>Event calendar</h1>
      <p class="muted">Click a day to schedule an event there, or an event to open it.</p>
    </div>
    <div class="card">
      <full-calendar [options]="options" />
    </div>
    <ul class="legend">
      @for (t of templates() ?? []; track t.key) {
        <li><span class="swatch" [style.background]="t.color"></span>{{ t.name }}</li>
      }
    </ul>
  `,
})
export class CalendarPage {
  private readonly api = inject(Api);
  private readonly router = inject(Router);

  protected readonly templates = toSignal(this.api.templates());

  protected readonly options: CalendarOptions = {
    plugins: [dayGridPlugin, listPlugin, interactionPlugin],
    initialView: 'dayGridMonth',
    headerToolbar: { left: 'prev,next today', center: 'title', right: 'dayGridMonth,listMonth' },
    buttonText: { listMonth: 'agenda' },
    height: 'auto',
    eventDisplay: 'block',
    // The API returns store wall-clock times without an offset, which FullCalendar shows as-is.
    events: (info: { start: Date; end: Date }) =>
      firstValueFrom(this.api.events(toIsoDate(info.start), toIsoDate(info.end))).then((events) =>
        events.map(
          (e): EventInput => ({
            id: String(e.id),
            title: `${e.name} (${e.registeredCount}/${e.capacity})`,
            start: e.startLocal,
            end: e.endLocal,
            color: e.color,
          }),
        ),
      ),
    eventClick: (info) => this.router.navigate(['/events', info.event.id]),
    dateClick: (info) => this.router.navigate(['/events/new'], { queryParams: { date: info.dateStr } }),
  };
}
