import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { IGameTemplate } from '../interfaces/responses/IGameTemplate';

@Injectable({ providedIn: 'root' })
export class TemplateService {
  private readonly http = inject(HttpClient);

  getTemplates() {
    return this.http.get<IGameTemplate[]>('/api/templates');
  }
}
