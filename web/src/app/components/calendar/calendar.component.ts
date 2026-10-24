import { Component, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { Router } from '@angular/router';
import { FullCalendarModule } from '@fullcalendar/angular';
import { CalendarOptions, EventInput } from '@fullcalendar/core';
import dayGridPlugin from '@fullcalendar/daygrid';
import interactionPlugin from '@fullcalendar/interaction';
import listPlugin from '@fullcalendar/list';
import { firstValueFrom } from 'rxjs';
import { toIsoDate } from '../../helpers/date.helper';
import { EventService } from '../../services/event.service';
import { TemplateService } from '../../services/template.service';

@Component({
  selector: 'app-calendar',
  imports: [FullCalendarModule],
  templateUrl: './calendar.component.html',
  styleUrl: './calendar.component.scss',
})
export class CalendarComponent {
  private readonly eventService = inject(EventService);
  private readonly router = inject(Router);

  protected readonly templates = toSignal(inject(TemplateService).getTemplates());

  protected readonly options: CalendarOptions = {
    plugins: [dayGridPlugin, listPlugin, interactionPlugin],
    initialView: 'dayGridMonth',
    headerToolbar: { left: 'prev,next today', center: 'title', right: 'dayGridMonth,listMonth' },
    buttonText: { listMonth: 'agenda' },
    height: 'auto',
    eventDisplay: 'block',
    // The API returns store wall-clock times without an offset, which FullCalendar shows as-is.
    events: (info: { start: Date; end: Date }) =>
      firstValueFrom(this.eventService.getEvents(toIsoDate(info.start), toIsoDate(info.end))).then((events) =>
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
