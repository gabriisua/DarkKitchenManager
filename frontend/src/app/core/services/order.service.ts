import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { PagedResponse, OrderResponseDto, OrderStatus } from '../../shared/models/api.models';

@Injectable({
  providedIn: 'root'
})
export class OrderService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/order`;

  getPaged(query: any): Observable<PagedResponse<OrderResponseDto>> {
    let params = new HttpParams()
      .set('page', query.page)
      .set('pageSize', query.pageSize);

    // Filtri
    if (query.status) params = params.set('status', query.status);
    if (query.customerId) params = params.set('customerId', query.customerId);
    if (query.dateFrom) params = params.set('dateFrom', query.dateFrom);
    if (query.dateTo) params = params.set('dateTo', query.dateTo);

    // --- ORDINAMENTO ---
    if (query.sortColumn) params = params.set('sortColumn', query.sortColumn);
    if (query.sortDirection) params = params.set('sortDirection', query.sortDirection);

    return this.http.get<any>(this.baseUrl, { params }).pipe(
      map(response => response.data)
    );
  }

  getById(id: string): Observable<OrderResponseDto> {
    return this.http.get<any>(`${this.baseUrl}/${id}`).pipe(
      map(response => response.data || response)
    );
  }

  // ==========================================
  // --- CREAZIONE NUOVO ORDINE ---
  // ==========================================
  createOrder(payload: any): Observable<OrderResponseDto> {
    return this.http.post<any>(this.baseUrl, payload).pipe(
      map(response => response.data)
    );
  }

  updateStatus(id: string, status: OrderStatus): Observable<boolean> {
    return this.http.patch<any>(`${this.baseUrl}/${id}/status`, { status }).pipe(
      // Se il backend usa il wrapper anche per il patch, lo estraiamo.
      // Altrimenti restituiamo true come default in caso di successo HTTP 200.
      map(response => response.data ?? true)
    );
  }

  // ==========================================
  // --- DOWNLOAD DOCUMENTI ---
  // ==========================================
  downloadDdt(orderId: string): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/${orderId}/ddt`, {
      responseType: 'blob' // <-- FONDAMENTALE per scaricare file!
    });
  }
}
