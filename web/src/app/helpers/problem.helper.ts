import { HttpErrorResponse } from '@angular/common/http';
import { IProblemDetails } from '../interfaces/responses/IProblemDetails';

/** Extracts the server's ProblemDetails from a failed request so the UI can show its message. */
export function toProblemDetails(err: unknown): IProblemDetails {
  if (err instanceof HttpErrorResponse && err.error && typeof err.error === 'object') {
    return err.error as IProblemDetails;
  }
  return { detail: 'Something went wrong. Please try again.' };
}
