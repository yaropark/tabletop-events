import { IEventSummary } from './IEventSummary';

/** The summary plus everything the event and registration pages show. */
export interface IEventDetails extends IEventSummary {
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
