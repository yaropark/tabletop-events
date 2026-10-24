import { DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { BehaviorSubject, catchError, of, switchMap } from 'rxjs';
import { Api, Problem, RegistrationResponse, toProblem } from './api';

@Component({
  selector: 'app-register-page',
  imports: [DatePipe, ReactiveFormsModule],
  template: `
    @if (event(); as e) {
      <div class="card narrow">
        <p class="tag" [style.background]="e.color">{{ e.gameName }} · {{ e.format }}</p>
        <h1>{{ e.name }}</h1>
        <p>{{ e.startLocal | date: 'EEEE, MMMM d · h:mm a' }}</p>
        <p class="muted">{{ e.location }}</p>

        @if (registered(); as r) {
          <p class="alert success">
            You're in, {{ r.name }}! See you there.
            {{ r.seatsLeft }} seat{{ r.seatsLeft === 1 ? '' : 's' }} left.
          </p>
          <a class="button secondary" [href]="api.icsUrl(e.id)" download>Add to my calendar</a>
        } @else {
          <!-- The UI hint is a convenience; the server makes the actual capacity decision. -->
          @if (!e.registrationOpen) {
            <p class="alert error">
              {{ e.seatsLeft === 0 ? 'Sorry, this event is full.' : 'Registration is closed.' }}
            </p>
          } @else {
            <p>{{ e.seatsLeft }} of {{ e.capacity }} seats left.</p>
            <form class="form" [formGroup]="form" (ngSubmit)="submit(e.id)">
              <label>
                Your name
                <input formControlName="name" maxlength="100" autocomplete="name" />
                <small class="error">{{ problem()?.errors?.['name'] }}</small>
              </label>
              <label>
                <span>Email <span class="muted">(optional)</span></span>
                <input type="email" formControlName="email" autocomplete="email" />
                <small class="error">{{ problem()?.errors?.['email'] }}</small>
              </label>
              @if (problem()?.detail; as detail) {
                <p class="alert error">{{ detail }}</p>
              }
              <button type="submit" [disabled]="form.invalid || saving()">Register</button>
            </form>
          }
        }
      </div>
    } @else if (event() === null) {
      <p class="alert error">Event not found.</p>
    }
  `,
})
export class RegisterPage {
  protected readonly api = inject(Api);
  private readonly id = Number(inject(ActivatedRoute).snapshot.paramMap.get('id'));

  private readonly reload = new BehaviorSubject<void>(undefined);
  protected readonly event = toSignal(
    this.reload.pipe(switchMap(() => this.api.event(this.id).pipe(catchError(() => of(null))))),
  );
  protected readonly registered = signal<RegistrationResponse | null>(null);
  protected readonly problem = signal<Problem | null>(null);
  protected readonly saving = signal(false);

  protected readonly form = inject(FormBuilder).nonNullable.group({
    name: ['', Validators.required],
    email: ['', Validators.email],
  });

  protected submit(id: number) {
    const { name, email } = this.form.getRawValue();
    this.saving.set(true);
    this.problem.set(null);
    this.api.register(id, name, email).subscribe({
      next: (r) => this.registered.set(r),
      error: (err) => {
        this.problem.set(toProblem(err));
        this.saving.set(false);
        // Seats may have changed since the page loaded (e.g. someone took the last one).
        this.reload.next();
      },
    });
  }
}
