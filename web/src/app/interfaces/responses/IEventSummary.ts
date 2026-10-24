import { IEventBase } from '../base/IEventBase';

/** What the calendar needs per event. */
export interface IEventSummary extends IEventBase {
  id: number;
  gameName: string;
  color: string;
  /** Store wall-clock time without offset, e.g. 2026-10-30T18:00:00. */
  startLocal: string;
  endLocal: string;
  capacity: number;
  registeredCount: number;
}
