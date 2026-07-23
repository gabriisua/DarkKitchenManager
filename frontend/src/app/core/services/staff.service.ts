import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { StaffPagedResponse, StaffFilter, StaffUpdateRequest, StaffCreateRequest, Staff, StaffPagedRequest, PagedResponse } from '../../shared/models/api.models';
import { buildPagedParams } from '../utils/http-params.util';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class StaffService {
  private apiUrl = `${environment.apiUrl}/Staff`;

  constructor(private http: HttpClient) {}

  getStaff(filter: StaffFilter): Observable<any> {
    let params = new HttpParams()
      .set('Page', filter.page.toString())
      .set('PageSize', filter.pageSize.toString());

    if (filter.search) params = params.set('Search', filter.search);
    if (filter.email) params = params.set('Email', filter.email);
    if (filter.role) params = params.set('Role', filter.role);
    if (filter.sortColumn) params = params.set('SortColumn', filter.sortColumn);
    if (filter.sortDirection) params = params.set('SortDirection', filter.sortDirection);
    if (filter.dateFrom) params = params.set('DateFrom', filter.dateFrom);
    if (filter.dateTo) params = params.set('DateTo', filter.dateTo);

    return this.http.get<any>(this.apiUrl, { params }).pipe(
      map((res: any) => {

        if (res && res.succeeded && res.data && res.data.items) {

          res.data.items = res.data.items.map((staff: any) => {

            if (staff.lastLogin && !staff.lastLogin.endsWith('Z')) {

              staff.lastLogin = `${staff.lastLogin}Z`;
            }
            return staff;
          });
        }
        return res;
      })
    );
  }

  getPaged(params: StaffPagedRequest): Observable<PagedResponse<Staff>> {
    return this.http.get<any>(this.apiUrl, { params: buildPagedParams(params) }).pipe(
      map((res: any) => {
        if (res.succeeded && res.data) {
          if (res.data.items) {
            res.data.items = res.data.items.map((item: any) => {
              if (item.lastLogin && !item.lastLogin.endsWith('Z')) {
                item.lastLogin = `${item.lastLogin}Z`;
              }
              return item;
            });
          }
          return res.data as PagedResponse<Staff>;
        }
        throw new Error(res.message || 'Failed to load data');
      })
    );
  }

  createStaff(data: StaffCreateRequest | any): Observable<void> {
    return this.http.post<void>(this.apiUrl, data);
  }

  updateStaff(id: string, data: StaffUpdateRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, data);
  }

  deleteStaff(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
