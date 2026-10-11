import { inject } from '@angular/core';
import { HttpInterceptorFn } from '@angular/common/http';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const token = localStorage.getItem('token');
  const router = inject(Router);

  if (!token) {
    return next(req);
  }

  const clonedRequest = req.clone({
    setHeaders: {
      Authorization: `Bearer ${token}`
    }
  });

  return next(clonedRequest).pipe(
    catchError((error: unknown) => {
      if (typeof error === 'object' && error !== null && 'status' in error && error.status === 401 && !req.url.includes('/api/auth/')) {
        localStorage.removeItem('token');
        void router.navigate(['/login']);
      }
      return throwError(() => error);
    })
  );
};
