import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const publicEndpoints = ['login', 'reset-password', 'forgot-password'];

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401 || error.status === 403) {
        const isPublic = publicEndpoints.some(url => req.url.includes(url));
        if (!isPublic) {
          authService.handleAuthFailure();
        }
      }
      return throwError(() => error);
    })
  );
};
