import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { buildPagedParams } from '../utils/http-params.util'; // Adatta il path se necessario
import {
  ClientCategoryDiscount,
  ClientPlateDiscount,
  SetCategoryDiscountPayload,
  SetPlateDiscountPayload,
  DiscountPagedRequest,
  PagedResponse,
  PlateDiscountDto,
  CategoryDiscountDto
} from '../../shared/models/api.models';

@Injectable({ providedIn: 'root' })
export class SaleService {
  private http = inject(HttpClient);

  // Base URL per il controller degli sconti
  private apiUrl = `${environment.apiUrl}/ClientDiscount`;

  // Signals per gestire lo stato locale nel componente (se lo desideri)
  readonly categoryDiscounts = signal<ClientCategoryDiscount[]>([]);
  readonly plateDiscounts = signal<ClientPlateDiscount[]>([]);

  // ==========================================
  // --- GRIGLIE GLOBALI (PAGINATE) ---
  // ==========================================

  getPagedPlateDiscounts(params: DiscountPagedRequest): Observable<PagedResponse<PlateDiscountDto>> {
    return this.http.get<any>(`${this.apiUrl}/plates/paged`, { params: buildPagedParams(params) }).pipe(
      map(res => {
        if (res.succeeded && res.data) return res.data as PagedResponse<PlateDiscountDto>;
        throw new Error(res.message || 'Errore nel caricamento degli sconti piatti');
      })
    );
  }

  getPagedCategoryDiscounts(params: DiscountPagedRequest): Observable<PagedResponse<CategoryDiscountDto>> {
    return this.http.get<any>(`${this.apiUrl}/categories/paged`, { params: buildPagedParams(params) }).pipe(
      map(res => {
        if (res.succeeded && res.data) return res.data as PagedResponse<CategoryDiscountDto>;
        throw new Error(res.message || 'Errore nel caricamento degli sconti categorie');
      })
    );
  }

  // ==========================================
  // --- AZIONI SCONTI DI CATEGORIA ---
  // ==========================================

  getCategoryDiscounts(customerId: string): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/customers/${customerId}/categories`);
  }

  setCategoryDiscount(customerId: string, payload: SetCategoryDiscountPayload): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/customers/${customerId}/categories`, payload);
  }

  deleteCategoryDiscount(customerId: string, categoryId: number): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/customers/${customerId}/categories/${categoryId}`);
  }

  // ==========================================
  // --- AZIONI SCONTI SINGOLO PIATTO ---
  // ==========================================

  getPlateDiscounts(customerId: string): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/customers/${customerId}/plates`);
  }

  setPlateDiscount(customerId: string, payload: SetPlateDiscountPayload): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/customers/${customerId}/plates`, payload);
  }

  deletePlateDiscount(customerId: string, plateId: number): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/customers/${customerId}/plates/${plateId}`);
  }

  // ==========================================
  // --- TEST DEL MOTORE PREZZI ---
  // ==========================================

  getEffectivePrice(customerId: string, plateId: number): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/customers/${customerId}/plates/${plateId}/effective-price`);
  }
}
