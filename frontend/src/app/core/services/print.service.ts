import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PrintLabelRequest, PrintBatchItem } from '../../shared/models/api.models';

@Injectable({ providedIn: 'root' })
export class PrintService {
  private apiUrl = `${environment.apiUrl}/Print`;

  constructor(private http: HttpClient) {}

  // =======================================================================
  // ETICHETTE STANDARD
  // =======================================================================

  printSingleLabel(plateId: number, payload: PrintLabelRequest): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/standard/${plateId}/single`, payload);
  }

  printBatchLabels(payload: PrintBatchItem[]): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/standard/batch`, payload);
  }

  // =======================================================================
  // ETICHETTE CORTILIA
  // =======================================================================

  printCortiliaSingleLabel(plateId: number, payload: PrintLabelRequest): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/cortilia/${plateId}/single`, payload);
  }

  printCortiliaBatchLabels(payload: PrintBatchItem[]): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/cortilia/batch`, payload);
  }

  // =======================================================================
  // ETICHETTE FOORBAN
  // =======================================================================

  printFoorbanSingleLabel(plateId: number, payload: PrintLabelRequest): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/foorban/${plateId}/single`, payload);
  }

  printFoorbanBatchLabels(payload: PrintBatchItem[]): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/foorban/batch`, payload);
  }

  // =======================================================================
  // ETICHETTE CRIO (Cryogenic)
  // =======================================================================

  printCrioSingleLabel(plateId: number, payload: PrintLabelRequest): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/crio/${plateId}/single`, payload);
  }

  printCrioBatchLabels(payload: PrintBatchItem[]): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/crio/batch`, payload);
  }
}
