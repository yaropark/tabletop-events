import { DatePipe } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { BehaviorSubject, catchError, of, switchMap } from 'rxjs';
import { toProblemDetails } from '../../helpers/problem.helper';
import { IProblemDetails } from '../../interfaces/responses/IProblemDetails';
import { IRegistrationResult } from '../../interfaces/responses/IRegistrationResult';
import { EventService } from '../../services/event.service';
import { RegistrationService } from '../../services/registration.service';

@Component({
  selector: 'app-event-register',
  imports: [DatePipe, ReactiveFormsModule],
  templateUrl: './event-register.component.html',
  styleUrl: './event-register.component.scss',
})
export class EventRegisterComponent {
  protected readonly eventService = inject(EventService);
  private readonly registrationService = inject(RegistrationService);
  private readonly id = Number(inject(ActivatedRoute).snapshot.paramMap.get('id'));

  private readonly reload = new BehaviorSubject<void>(undefined);
  protected readonly event = toSignal(
    this.reload.pipe(switchMap(() => this.eventService.getEvent(this.id).pipe(catchError(() => of(null))))),
  );
  protected readonly registered = signal<IRegistrationResult | null>(null);
  protected readonly problem = signal<IProblemDetails | null>(null);
  protected readonly saving = signal(false);

  protected readonly form = inject(FormBuilder).nonNullable.group({
    name: ['', Validators.required],
    email: ['', Validators.email],
  });

  protected submit(id: number) {
    const { name, email } = this.form.getRawValue();
    this.saving.set(true);
    this.problem.set(null);
    this.registrationService.register(id, { name, email: email || null }).subscribe({
      next: (r) => this.registered.set(r),
      error: (err) => {
        this.problem.set(toProblemDetails(err));
        this.saving.set(false);
        // Seats may have changed since the page loaded (e.g. someone took the last one).
        this.reload.next();
      },
    });
  }
}
