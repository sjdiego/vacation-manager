import { Injectable } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';

export interface ApiError {
  status: number;
  message: string;
  details?: any;
  timestamp: Date;
}

@Injectable({
  providedIn: 'root'
})
export class ErrorHandlerService {
  handleError(error: HttpErrorResponse): Observable<never> {
    const apiError: ApiError = {
      status: error.status,
      message: this.getErrorMessage(error),
      details: error.error,
      timestamp: new Date()
    };

    console.error('API Error:', apiError);
    return throwError(() => apiError);
  }

  private getErrorMessage(error: HttpErrorResponse): string {
    if (error.error instanceof ErrorEvent) {
      return error.error.message;
    }

    switch (error.status) {
      case 0:
        return 'Unable to connect to the server. Please check your network connection.';
      case 400:
        return error.error?.message || 'Bad request. Please check your input.';
      case 401:
        return 'Unauthorized. Please log in again.';
      case 403:
        return 'Forbidden. You do not have permission to access this resource.';
      case 404:
        return 'Resource not found.';
      case 409:
        return error.error?.message || 'Conflict. The resource may already exist.';
      case 422:
        return error.error?.message || 'Validation error. Please check your input.';
      case 500:
        return 'Internal server error. Please try again later.';
      case 503:
        return 'Service unavailable. Please try again later.';
      default:
        return error.message || 'An unexpected error occurred.';
    }
  }
}
