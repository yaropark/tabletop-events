/** Fields shared by the create-event request and the event responses. */
export interface IEventBase {
  name: string;
  templateKey: string;
  format: string;
}
