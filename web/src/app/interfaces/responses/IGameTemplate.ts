export interface IGameTemplate {
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
