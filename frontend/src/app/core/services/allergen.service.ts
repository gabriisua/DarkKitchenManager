import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { Allergen, AllergenPagedRequest, PagedResponse } from '../../shared/models/api.models';
import { buildPagedParams } from '../utils/http-params.util';

@Injectable({ providedIn: 'root' })
export class AllergenService {
  private apiUrl = `${environment.apiUrl}/Allergen`;

  readonly allergens = signal<Allergen[]>([]);

  constructor(private http: HttpClient) {}

  getPaged(params: AllergenPagedRequest): Observable<PagedResponse<Allergen>> {
    return this.http.get<{ data: PagedResponse<Allergen> }>(this.apiUrl, { params: buildPagedParams(params) }).pipe(
      map(res => res.data)
    );
  }

  loadAll(): void {
    this.http.get<{ data: Allergen[] }>(this.apiUrl).subscribe({
      next: res => {
        if (res.data) {
          this.allergens.set(res.data);
        }
      }
    });
  }

  getAll(): Observable<{ data: Allergen[] }> {
    return this.http.get<{ data: Allergen[] }>(`${this.apiUrl}/all`);
  }

  getById(id: number): Observable<{ data: Allergen }> {
    return this.http.get<{ data: Allergen }>(`${this.apiUrl}/${id}`);
  }

  create(data: { name: string; code: string; description: string }): Observable<{ data: Allergen }> {
    return this.http.post<{ data: Allergen }>(this.apiUrl, data).pipe(
      tap(() => this.loadAll())
    );
  }

  update(id: number, data: { name: string; code: string; description: string }): Observable<{ data: Allergen }> {
    return this.http.put<{ data: Allergen }>(`${this.apiUrl}/${id}`, data).pipe(
      tap(() => this.loadAll())
    );
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`).pipe(
      tap(() => this.loadAll())
    );
  }
}
