import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';

export interface GameTemplate {
  key: string;
  name: string;
  category: string;
  color: string;
  formats: string[];
  defaultDurationMinutes: number;
  defaultCapacity: number;
  maxCapacity: number;
  minPlayers: number;
}

export interface EventSummary {
  id: number;
  name: string;
  templateKey: string;
  gameName: string;
  color: string;
  format: string;
  /** Store wall-clock time without offset, e.g. 2026-10-30T18:00:00. */
  startLocal: string;
  endLocal: string;
  capacity: number;
  registeredCount: number;
}

export interface EventDetails extends EventSummary {
  startUtc: string;
  endUtc: string;
  timeZone: string;
  durationMinutes: number;
  location: string;
  minPlayers: number;
  seatsLeft: number;
  registrationOpen: boolean;
  registrationUrl: string;
}

export interface CreateEventRequest {
  name: string;
  templateKey: string;
  startLocal: string;
  format: string;
  durationMinutes: number;
  capacity: number;
  location: string;
}

export interface Registrant {
  name: string;
  registeredUtc: string;
}

export interface RegistrationResponse {
  id: number;
  name: string;
  seatsLeft: number;
}

/** Shape of ASP.NET Core ProblemDetails / ValidationProblemDetails. */
export interface Problem {
  title?: string;
  detail?: string;
  errors?: Record<string, string[]>;
}

export function toProblem(err: unknown): Problem {
  if (err instanceof HttpErrorResponse && err.error && typeof err.error === 'object') {
    return err.error as Problem;
  }
  return { detail: 'Something went wrong. Please try again.' };
}

@Injectable({ providedIn: 'root' })
export class Api {
  private readonly http = inject(HttpClient);

  templates() {
    return this.http.get<GameTemplate[]>('/api/templates');
  }

  events(from: string, to: string) {
    return this.http.get<EventSummary[]>('/api/events', { params: { from, to } });
  }

  event(id: number) {
    return this.http.get<EventDetails>(`/api/events/${id}`);
  }

  createEvent(request: CreateEventRequest) {
    return this.http.post<EventDetails>('/api/events', request);
  }

  registrants(id: number) {
    return this.http.get<Registrant[]>(`/api/events/${id}/registrations`);
  }

  register(id: number, name: string, email: string) {
    return this.http.post<RegistrationResponse>(`/api/events/${id}/registrations`, {
      name,
      email: email || null,
    });
  }

  qrUrl(id: number) {
    return `/api/events/${id}/qr.png`;
  }

  icsUrl(id: number) {
    return `/api/events/${id}/invite.ics`;
  }
}
