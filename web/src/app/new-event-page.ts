import { Component, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Api, GameTemplate, Problem, toProblem } from './api';
import { toIsoDate } from './dates';

@Component({
  selector: 'app-new-event-page',
  imports: [ReactiveFormsModule],
  template: `
    <div class="page-head"><h1>New event</h1></div>

    <form class="card form" [formGroup]="form" (ngSubmit)="submit()">
      <label>
        Event name
        <input formControlName="name" maxlength="120" placeholder="Friday Night Magic" />
        <small class="error">{{ problem()?.errors?.['name'] }}</small>
      </label>

      <label>
        Game
        <select formControlName="templateKey">
          <option value="" disabled>Choose a game…</option>
          @for (t of templates() ?? []; track t.key) {
            <option [value]="t.key">{{ t.name }}</option>
          }
        </select>
        <small class="error">{{ problem()?.errors?.['templateKey'] }}</small>
      </label>

      @if (template(); as t) {
        <label>
          Format
          <select formControlName="format">
            @for (f of t.formats; track f) {
              <option [value]="f">{{ f }}</option>
            }
          </select>
          <small class="error">{{ problem()?.errors?.['format'] }}</small>
        </label>

        <div class="row">
          <label>
            Date
            <input type="date" formControlName="date" />
          </label>
          <label>
            Start time
            <input type="time" formControlName="time" />
          </label>
        </div>
        <small class="error">{{ problem()?.errors?.['startLocal'] }}</small>

        <div class="row">
          <label>
            Duration (minutes)
            <input type="number" formControlName="durationMinutes" min="15" step="15" />
            <small class="error">{{ problem()?.errors?.['durationMinutes'] }}</small>
          </label>
          <label>
            Player capacity
            <input type="number" formControlName="capacity" [min]="t.minPlayers" [max]="t.maxCapacity" />
            <small class="muted">{{ t.minPlayers }}–{{ t.maxCapacity }} players for {{ t.name }}</small>
            <small class="error">{{ problem()?.errors?.['capacity'] }}</small>
          </label>
        </div>

        <label>
          Location
          <input formControlName="location" placeholder="Defaults to the store address" />
        </label>
      }

      @if (problem()?.detail; as detail) {
        <p class="alert error">{{ detail }}</p>
      }
      <button type="submit" [disabled]="form.invalid || saving()">Create event</button>
    </form>
  `,
})
export class NewEventPage {
  private readonly api = inject(Api);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder).nonNullable;

  protected readonly templates = toSignal(this.api.templates());
  protected readonly problem = signal<Problem | null>(null);
  protected readonly saving = signal(false);

  protected readonly form = this.fb.group({
    name: ['', Validators.required],
    templateKey: ['', Validators.required],
    format: [''],
    date: [inject(ActivatedRoute).snapshot.queryParamMap.get('date') ?? toIsoDate(new Date()), Validators.required],
    time: ['18:00', Validators.required],
    durationMinutes: [0],
    capacity: [0],
    location: [''],
  });

  private readonly templateKey = toSignal(this.form.controls.templateKey.valueChanges, { initialValue: '' });
  protected readonly template = computed<GameTemplate | undefined>(() =>
    this.templates()?.find((t) => t.key === this.templateKey()),
  );

  constructor() {
    // Prefill game-specific fields from the selected template.
    this.form.controls.templateKey.valueChanges.pipe(takeUntilDestroyed()).subscribe((key) => {
      const t = this.templates()?.find((x) => x.key === key);
      if (!t) return;
      this.form.patchValue({
        format: t.formats[0],
        durationMinutes: t.defaultDurationMinutes,
        capacity: t.defaultCapacity,
      });
    });
  }

  protected submit() {
    const v = this.form.getRawValue();
    this.saving.set(true);
    this.problem.set(null);
    this.api
      .createEvent({
        name: v.name,
        templateKey: v.templateKey,
        format: v.format,
        startLocal: `${v.date}T${v.time}`,
        durationMinutes: v.durationMinutes,
        capacity: v.capacity,
        location: v.location,
      })
      .subscribe({
        next: (created) => this.router.navigate(['/events', created.id]),
        error: (err) => {
          this.problem.set(toProblem(err));
          this.saving.set(false);
        },
      });
  }
}
