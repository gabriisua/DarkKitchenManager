import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { buildPagedParams } from '../utils/http-params.util';
import {
  PagedResponse,
  PendingInvoiceSummary,
  PendingOrderItem,
  BulkInvoiceRequest,
  InvoicePendingSummaryRequest,
  InvoiceHistoryItem,
  InvoiceHistoryRequest
} from '../../shared/models/api.models';

@Injectable({
  providedIn: 'root'
})
export class InvoiceService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/invoice`;

  getPendingSummary(params: InvoicePendingSummaryRequest): Observable<PagedResponse<PendingInvoiceSummary>> {
    let httpParams = new HttpParams()
      .set('Page', params.page)
      .set('PageSize', params.pageSize);

    if (params.search) httpParams = httpParams.set('Search', params.search);
    if (params.dateFrom) httpParams = httpParams.set('DateFrom', params.dateFrom);
    if (params.dateTo) httpParams = httpParams.set('DateTo', params.dateTo);

    // Ritorniamo direttamente la GET senza fare il map su .data
    return this.http.get<PagedResponse<PendingInvoiceSummary>>(`${this.baseUrl}/pending-summary`, { params: httpParams });
  }

  getPendingOrders(customerId: string, dateFrom?: string, dateTo?: string): Observable<PendingOrderItem[]> {
    let httpParams = new HttpParams();

    if (dateFrom) httpParams = httpParams.set('DateFrom', dateFrom);
    if (dateTo) httpParams = httpParams.set('DateTo', dateTo);

    return this.http.get<any>(`${this.baseUrl}/pending-summary/${customerId}/orders`, { params: httpParams }).pipe(
      map(response => response.data || response)
    );
  }

  bulkInvoice(payload: BulkInvoiceRequest): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/bulk-invoice`, payload);
  }

  getInvoiceHistory(params: InvoiceHistoryRequest): Observable<PagedResponse<InvoiceHistoryItem>> {
    return this.http.get<any>(`${this.baseUrl}/history`, { params: buildPagedParams(params) }).pipe(
      map(res => res.data as PagedResponse<InvoiceHistoryItem>)
    );
  }

  deleteInvoice(ficDocumentId: number): Observable<boolean> {
    return this.http.delete<any>(`${this.baseUrl}/${ficDocumentId}`).pipe(
      map(res => res.data as boolean)
    );
  }

  getInvoicePdfUrl(ficDocumentId: number): Observable<string> {
    return this.http.get<any>(`${this.baseUrl}/${ficDocumentId}/pdf`).pipe(
      map(res => res.data as string)
    );
  }
}
