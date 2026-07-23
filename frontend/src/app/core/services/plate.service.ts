import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { PlateCreateRequest, FoodCost, NutritionInfo, Plate, PlatePagedRequest, PagedResponse } from '../../shared/models/api.models';
import { buildPagedParams } from '../utils/http-params.util';

@Injectable({ providedIn: 'root' })
export class PlateService {
  private apiUrl = `${environment.apiUrl}/Plate`;

  constructor(private http: HttpClient) {}

  getPaged(params: PlatePagedRequest): Observable<PagedResponse<Plate>> {
    return this.http.get<{ data: PagedResponse<Plate> }>(this.apiUrl, { params: buildPagedParams(params) }).pipe(
      map(res => res.data)
    );
  }

  create(data: PlateCreateRequest): Observable<{ data: Plate }> {
    return this.http.post<{ data: Plate }>(this.apiUrl, data);
  }

  getById(id: number): Observable<{ data: Plate }> {
    return this.http.get<{ data: Plate }>(`${this.apiUrl}/${id}`);
  }

  update(id: number, data: PlateCreateRequest | any): Observable<{ data: Plate }> {
    return this.http.put<{ data: Plate }>(`${this.apiUrl}/${id}`, data);
  }

  delete(id: number): Observable<{ data: boolean }> {
    return this.http.delete<{ data: boolean }>(`${this.apiUrl}/${id}`);
  }

  getFoodCost(id: number): Observable<{ data: FoodCost }> {
    return this.http.get<{ data: FoodCost }>(`${this.apiUrl}/${id}/food-cost`);
  }

  getNutrition(id: number): Observable<{ data: NutritionInfo }> {
    return this.http.get<{ data: NutritionInfo }>(`${this.apiUrl}/${id}/nutrition`);
  }

  // --- SCHEDA TECNICA ---
  downloadTechnicalSheet(id: number): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/${id}/technical-sheet`, {
      responseType: 'blob'
    });
  }
}
