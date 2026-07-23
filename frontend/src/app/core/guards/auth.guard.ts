import { inject } from '@angular/core';
import { CanMatchFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const authGuard: CanMatchFn = () => {
  const router = inject(Router);
  const auth = inject(AuthService);
  const token = auth.getToken();

  if (!token) {
    auth.clearToken();
    return router.createUrlTree(['/']);
  }

  return true;
};
