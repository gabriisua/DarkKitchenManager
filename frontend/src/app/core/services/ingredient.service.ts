import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { Ingredient, IngredientCreateRequest, IngredientPagedRequest, PagedResponse } from '../../shared/models/api.models';
import { buildPagedParams } from '../utils/http-params.util';

@Injectable({ providedIn: 'root' })
export class IngredientService {
  private apiUrl = `${environment.apiUrl}/Ingredient`;

  readonly ingredients = signal<Ingredient[]>([]);

  constructor(private http: HttpClient) {}

  // ==========================================
  // GET: Recupera lista paginata con filtri
  // ==========================================
  getPaged(params: IngredientPagedRequest): Observable<PagedResponse<Ingredient>> {
    return this.http.get<{ data: PagedResponse<Ingredient> }>(this.apiUrl, { params: buildPagedParams(params) }).pipe(
      map(res => res.data)
    );
  }

  // ==========================================
  // GET: Carica tutti gli elementi nel Signal
  // ==========================================
  loadAll(): void {
    this.http.get<{ data: Ingredient[] }>(this.apiUrl).subscribe({
      next: res => {
        if (res.data) {
          this.ingredients.set(res.data);
        }
      }
    });
  }

  // ==========================================
  // GET: Recupera tutti gli ingredienti
  // ==========================================
  getAll(): Observable<{ data: Ingredient[] }> {
    return this.http.get<{ data: Ingredient[] }>(this.apiUrl);
  }

  // ==========================================
  // GET by ID: Recupera singolo ingrediente
  // ==========================================
  getById(id: number): Observable<{ data: Ingredient }> {
    return this.http.get<{ data: Ingredient }>(`${this.apiUrl}/${id}`);
  }

  // ==========================================
  // POST: Crea un nuovo ingrediente
  // ==========================================
  create(data: IngredientCreateRequest): Observable<{ data: Ingredient }> {
    return this.http.post<{ data: Ingredient }>(this.apiUrl, data);
  }

  // ==========================================
  // PUT: Aggiorna un ingrediente esistente
  // ==========================================
  // Nota: Sostituisci "IngredientCreateRequest" con un eventuale
  // "IngredientUpdateRequest" se i tuoi modelli API lo prevedono.
  update(id: number, data: IngredientCreateRequest | Partial<Ingredient>): Observable<{ data: Ingredient }> {
    return this.http.put<{ data: Ingredient }>(`${this.apiUrl}/${id}`, data);
  }

  // ==========================================
  // DELETE: Elimina un ingrediente
  // ==========================================
  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
