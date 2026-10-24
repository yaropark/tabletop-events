import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { IRegisterRequest } from '../interfaces/requests/IRegisterRequest';
import { IRegistrant } from '../interfaces/responses/IRegistrant';
import { IRegistrationResult } from '../interfaces/responses/IRegistrationResult';

@Injectable({ providedIn: 'root' })
export class RegistrationService {
  private readonly http = inject(HttpClient);

  private url(eventId: number) {
    return `/api/events/${eventId}/registrations`;
  }

  getRegistrants(eventId: number) {
    return this.http.get<IRegistrant[]>(this.url(eventId));
  }

  /** Capacity and duplicates are enforced by the server; a 409 carries the message to show. */
  register(eventId: number, request: IRegisterRequest) {
    return this.http.post<IRegistrationResult>(this.url(eventId), request);
  }
}
