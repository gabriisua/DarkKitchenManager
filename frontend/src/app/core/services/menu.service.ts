import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import {
  Menu, MenuDetail, MenuCreateRequest, MenuPagedRequest, PagedResponse
} from '../../shared/models/api.models';
import { buildPagedParams } from '../utils/http-params.util';

@Injectable({ providedIn: 'root' })
export class MenuService {
  private apiUrl = `${environment.apiUrl}/Menu`;

  constructor(private http: HttpClient) {}

  getPaged(params: MenuPagedRequest): Observable<PagedResponse<Menu>> {
    return this.http.get<{ data: PagedResponse<Menu> }>(this.apiUrl, { params: buildPagedParams(params) }).pipe(
      map(res => res.data)
    );
  }

  getById(id: number): Observable<{ data: MenuDetail }> {
    return this.http.get<{ data: MenuDetail }>(`${this.apiUrl}/${id}`);
  }

  create(data: MenuCreateRequest): Observable<{ data: Menu }> {
    return this.http.post<{ data: Menu }>(this.apiUrl, data);
  }

  update(id: number, data: MenuCreateRequest): Observable<{ data: Menu }> {
    return this.http.put<{ data: Menu }>(`${this.apiUrl}/${id}`, data);
  }

  delete(id: number): Observable<{ data: boolean }> {
    return this.http.delete<{ data: boolean }>(`${this.apiUrl}/${id}`);
  }

  downloadMenuPdf(id: number): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/${id}/pdf`, { responseType: 'blob' });
  }

  downloadClassicLabel(menuId: number, plateId: number): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/${menuId}/items/${plateId}/label/classic`, { responseType: 'blob' });
  }

  downloadCustomLabel(menuId: number, plateId: number): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/${menuId}/items/${plateId}/label/custom`, { responseType: 'blob' });
  }
}
