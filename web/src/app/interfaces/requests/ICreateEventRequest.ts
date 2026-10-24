import { IEventBase } from '../base/IEventBase';

export interface ICreateEventRequest extends IEventBase {
  /** Store wall-clock time, e.g. 2026-10-30T18:00. */
  startLocal: string;
  durationMinutes: number;
  capacity: number;
  /** Empty means the store address. */
  location: string;
}
