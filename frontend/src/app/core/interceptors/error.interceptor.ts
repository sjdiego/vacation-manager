import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { ErrorHandlerService } from '../services/error-handler.service';
import { catchError } from 'rxjs';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const errorHandler = inject(ErrorHandlerService);

  return next(req).pipe(
    catchError((error) => {
      if (error.status === 401) {
        // Token expired, redirect to login
        router.navigate(['/login']);
      } else {
        errorHandler.handleError(error);
      }
      return errorHandler.handleError(error);
    })
  );
};
