import { Injectable, inject, signal } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { Customer, CustomerFilter, CustomerPagedRequest, PagedResponse } from '../../shared/models/api.models';
import { buildPagedParams } from '../utils/http-params.util';

@Injectable({ providedIn: 'root' })
export class CustomerService {
  private http = inject(HttpClient);

  // Base URL per i clienti (es: /api/Customer)
  private apiUrl = `${environment.apiUrl}/Customer`;

  // Base URL per le rotte degli hub logistici (es: /api/customers)
  private hubsBaseUrl = `${environment.apiUrl}/customers`;

  readonly customers = signal<Customer[]>([]);
  readonly totalCount = signal<number>(0);

  // ==========================================
  // --- METODI PER I CUSTOMER ---
  // ==========================================

  getCustomers(filter: CustomerFilter): Observable<any> {
    let params = new HttpParams()
      .set('Page', filter.page.toString())
      .set('PageSize', filter.pageSize.toString());

    if (filter.search) params = params.set('Search', filter.search);
    if (filter.type) params = params.set('Type', filter.type);
    if (filter.isActive !== undefined && filter.isActive !== null) {
      params = params.set('IsActive', filter.isActive.toString());
    }
    if (filter.sortColumn) params = params.set('SortColumn', filter.sortColumn);
    if (filter.sortDirection) params = params.set('SortDirection', filter.sortDirection);

    return this.http.get<any>(this.apiUrl, { params });
  }

  getPaged(params: CustomerPagedRequest): Observable<PagedResponse<Customer>> {
    return this.http.get<any>(this.apiUrl, { params: buildPagedParams(params) }).pipe(
      map((res: any) => {
        if (res.succeeded && res.data) {
          return res.data as PagedResponse<Customer>;
        }
        throw new Error(res.message || 'Failed to load data');
      })
    );
  }

  getCustomerById(id: string): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/${id}`);
  }

  createCustomer(data: any): Observable<any> {
    return this.http.post<any>(this.apiUrl, data);
  }

  updateCustomer(id: string, data: any): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/${id}`, data);
  }

  deleteCustomer(id: string): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/${id}`);
  }

  // ==========================================
  // --- METODI PER I DELIVERY HUB ---
  // ==========================================

  getCustomerHubs(customerId: string): Observable<any> {
    return this.http.get<any>(`${this.hubsBaseUrl}/${customerId}/hubs`);
  }

  createHub(customerId: string, data: any): Observable<any> {
    return this.http.post<any>(`${this.hubsBaseUrl}/${customerId}/hubs`, data);
  }

  updateHub(customerId: string, hubId: string, data: any): Observable<any> {
    return this.http.put<any>(`${this.hubsBaseUrl}/${customerId}/hubs/${hubId}`, data);
  }

  deleteHub(customerId: string, hubId: string): Observable<any> {
    return this.http.delete<any>(`${this.hubsBaseUrl}/${customerId}/hubs/${hubId}`);
  }
}
