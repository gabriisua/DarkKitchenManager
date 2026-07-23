import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { Category, CategoryCreateRequest, CategoryUpdateRequest, CategoryPagedRequest, PagedResponse } from '../../shared/models/api.models';

@Injectable({ providedIn: 'root' })
export class CategoryService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/Category`;

  getAllActive(): Observable<Category[]> {
    return this.http.get<{ data: Category[] }>(`${this.apiUrl}/active`).pipe(map(res => res.data || []));
  }

  getPaged(params: CategoryPagedRequest): Observable<PagedResponse<Category>> {
    let httpParams = new HttpParams()
      .set('page', params.page.toString())
      .set('pageSize', params.pageSize.toString());

    if (params.search) httpParams = httpParams.set('search', params.search);

    return this.http.get<{ data: PagedResponse<Category> }>(this.apiUrl, { params: httpParams }).pipe(map(res => res.data));
  }

  create(data: CategoryCreateRequest): Observable<number> {
    return this.http.post<{ data: number }>(this.apiUrl, data).pipe(map(res => res.data));
  }

  update(id: number, data: CategoryUpdateRequest): Observable<boolean> {
    return this.http.put<{ data: boolean }>(`${this.apiUrl}/${id}`, data).pipe(map(res => res.data));
  }

  delete(id: number): Observable<boolean> {
    return this.http.delete<{ data: boolean }>(`${this.apiUrl}/${id}`).pipe(map(res => res.data));
  }
}
