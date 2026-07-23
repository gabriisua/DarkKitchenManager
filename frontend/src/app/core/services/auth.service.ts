import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { LoginRequest, AuthResponse } from '../../shared/models/api.models';
import { Router } from '@angular/router';


@Injectable({ providedIn: 'root' })
export class AuthService {

  private apiUrl = `${environment.apiUrl}/Auth`;

  readonly currentUser = signal<any | null>(null);
  readonly isAuthenticated = signal<boolean>(false);

  constructor(
    private http: HttpClient,
    private router: Router
  ) {}

  init(): void {
    if (this.hasValidToken()) {
      this.isAuthenticated.set(true);
      this.loadUser();
    } else {
      this.clearToken();
    }
  }

  login(data: LoginRequest): Observable<AuthResponse> {
    return this.http.post<any>(`${this.apiUrl}/login`, data, {
      withCredentials: true
    }).pipe(

      map(res => res.data ? res.data : res),
      
      tap((parsedRes: AuthResponse) => {
        if (parsedRes && parsedRes.token) {
          this.setToken(parsedRes.token);
          this.isAuthenticated.set(true);
        }
      })
    );
  }

  loadUser(): void {
    const token = this.getToken();
    if (!token) return;

    this.http.get<{ data: any }>(`${this.apiUrl}/me`, {
      headers: { Authorization: `Bearer ${token}` }
    }).subscribe({
      next: res => this.currentUser.set(res.data),
      error: () => this.currentUser.set(null)
    });
  }

  setToken(token: string): void {
    localStorage.setItem('x-auth-token', token);
  }

  getToken(): string | null {
    const token = localStorage.getItem('x-auth-token');
    if (!token) return null;

    if (this.isTokenExpired(token)) {
      this.clearToken();
      return null;
    }

    return token;
  }

  clearToken(): void {
    localStorage.removeItem('x-auth-token');
    this.isAuthenticated.set(false);
    this.currentUser.set(null);
  }

  hasValidToken(): boolean {
    return !!this.getToken();
  }

  private decodeTokenPayload(token: string): any {
    try {
      const parts = token.split('.');
      if (parts.length !== 3) return null;

      const payloadBase64 = parts[1]
        .replace(/-/g, '+')
        .replace(/_/g, '/');

      const base64 = payloadBase64 + '='.repeat((4 - payloadBase64.length % 4) % 4);
      return JSON.parse(atob(base64));
    } catch {
      return null;
    }
  }

  private isTokenExpired(token: string): boolean {
    const payload = this.decodeTokenPayload(token);
    if (!payload?.exp) return true;

    return Date.now() >= payload.exp * 1000;
  }

  handleAuthFailure(): void {
    this.clearToken();
    this.router.navigate(['/']);
  }

  resetPasswordRequest(email: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/reset-password-request`, { email });
  }

  resetPasswordSubmit(data: { token: string; password: string }): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/reset-password-confirm`, data);
  }

  logout(): void {
    this.clearToken();
    this.router.navigate(['/']);
  }
}
