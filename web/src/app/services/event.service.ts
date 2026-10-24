import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { ICreateEventRequest } from '../interfaces/requests/ICreateEventRequest';
import { IEventDetails } from '../interfaces/responses/IEventDetails';
import { IEventSummary } from '../interfaces/responses/IEventSummary';

@Injectable({ providedIn: 'root' })
export class EventService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/events';

  /** Events starting in a store-local date range (yyyy-MM-dd). */
  getEvents(from: string, to: string) {
    return this.http.get<IEventSummary[]>(this.baseUrl, { params: { from, to } });
  }

  getEvent(id: number) {
    return this.http.get<IEventDetails>(`${this.baseUrl}/${id}`);
  }

  createEvent(request: ICreateEventRequest) {
    return this.http.post<IEventDetails>(this.baseUrl, request);
  }

  /** Image URL for the registration QR code; used directly in <img src>. */
  getQrCodeUrl(id: number) {
    return `${this.baseUrl}/${id}/qr.png`;
  }

  /** Download URL for the .ics calendar invite; used directly in <a href download>. */
  getCalendarInviteUrl(id: number) {
    return `${this.baseUrl}/${id}/invite.ics`;
  }
}
