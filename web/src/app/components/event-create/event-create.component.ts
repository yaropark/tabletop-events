import { Component, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { toIsoDate } from '../../helpers/date.helper';
import { toProblemDetails } from '../../helpers/problem.helper';
import { IGameTemplate } from '../../interfaces/responses/IGameTemplate';
import { IProblemDetails } from '../../interfaces/responses/IProblemDetails';
import { EventService } from '../../services/event.service';
import { TemplateService } from '../../services/template.service';

@Component({
  selector: 'app-event-create',
  imports: [ReactiveFormsModule],
  templateUrl: './event-create.component.html',
  styleUrl: './event-create.component.scss',
})
export class EventCreateComponent {
  private readonly eventService = inject(EventService);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder).nonNullable;

  protected readonly templates = toSignal(inject(TemplateService).getTemplates());
  protected readonly problem = signal<IProblemDetails | null>(null);
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
  protected readonly template = computed<IGameTemplate | undefined>(() =>
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
    this.eventService
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
          this.problem.set(toProblemDetails(err));
          this.saving.set(false);
        },
      });
  }
}
