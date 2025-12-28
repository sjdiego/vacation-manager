import { Injectable } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';

export interface ApiError {
  status: number;
  message: string;
  details?: any;
  timestamp: Date;
  validationErrors?: Record<string, string[]>;
}

export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
  extensions?: Record<string, any>;
  errors?: Record<string, string[]>;
  traceId?: string;
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

    // Extract validation errors from Problem Details if present
    if (this.isProblemDetails(error.error)) {
      const problemDetails = error.error as ProblemDetails;
      if (problemDetails.errors) {
        apiError.validationErrors = problemDetails.errors;
      }
      // Use the detail from Problem Details if available
      if (problemDetails.detail) {
        apiError.message = problemDetails.detail;
      }
    }

    console.error('API Error:', apiError);
    return throwError(() => apiError);
  }

  private isProblemDetails(error: any): error is ProblemDetails {
    return error && typeof error === 'object' && 
           ('type' in error || 'title' in error || 'detail' in error);
  }

  private getErrorMessage(error: HttpErrorResponse): string {
    if (error.error instanceof ErrorEvent) {
      return error.error.message;
    }

    // Try to extract message from Problem Details
    if (this.isProblemDetails(error.error)) {
      const problemDetails = error.error as ProblemDetails;
      if (problemDetails.detail) {
        return problemDetails.detail;
      }
      if (problemDetails.title) {
        return problemDetails.title;
      }
    }

    // Fallback to generic messages based on status code
    switch (error.status) {
      case 0:
        return 'Unable to connect to the server. Please check your network connection.';
      case 400:
        return error.error?.message || error.error?.detail || 'Bad request. Please check your input.';
      case 401:
        return 'Unauthorized. Please log in again.';
      case 403:
        return 'Forbidden. You do not have permission to access this resource.';
      case 404:
        return 'Resource not found.';
      case 409:
        return error.error?.message || error.error?.detail || 'Conflict. The resource may already exist.';
      case 422:
        return error.error?.message || error.error?.detail || 'Validation error. Please check your input.';
      case 500:
        return 'Internal server error. Please try again later.';
      case 503:
        return 'Service unavailable. Please try again later.';
      default:
        return error.message || 'An unexpected error occurred.';
    }
  }
}
