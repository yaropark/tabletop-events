/** ASP.NET Core ProblemDetails / ValidationProblemDetails returned for 4xx responses. */
export interface IProblemDetails {
  title?: string;
  detail?: string;
  /** Validation messages keyed by request field name, e.g. errors['capacity']. */
  errors?: Record<string, string[]>;
}
