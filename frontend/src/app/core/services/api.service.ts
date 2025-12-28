import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '@environments/environment';

/**
 * Interface matching the backend's ApiResponse wrapper for successful responses
 */
export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  message?: string;
  meta?: Record<string, any>;
}

/**
 * Service for making HTTP requests to the API.
 * 
 * Note: This service only handles successful responses wrapped in ApiResponse<T>.
 * Error responses (4xx/5xx) from the backend follow RFC 7807 Problem Details format
 * and are handled by the ErrorInterceptor before reaching this service.
 * 
 * Backend response patterns:
 * - Success (2xx): { success: true, data: T, message?: string }
 * - Error (4xx/5xx): { type, title, status, detail, instance, errors?, extensions? }
 */
@Injectable({
  providedIn: 'root'
})
export class ApiService {
  protected apiUrl = environment.apiUrl;

  constructor(protected http: HttpClient) {}

  get<T>(endpoint: string, params?: HttpParams | Record<string, string | string[]>): Observable<T> {
    return this.http.get<ApiResponse<T>>(`${this.apiUrl}${endpoint}`, { params }).pipe(
      map(response => this.unwrapResponse(response))
    );
  }

  post<T>(endpoint: string, body: any): Observable<T> {
    return this.http.post<ApiResponse<T>>(`${this.apiUrl}${endpoint}`, body).pipe(
      map(response => this.unwrapResponse(response))
    );
  }

  put<T>(endpoint: string, body: any): Observable<T> {
    return this.http.put<ApiResponse<T>>(`${this.apiUrl}${endpoint}`, body).pipe(
      map(response => this.unwrapResponse(response))
    );
  }

  delete<T>(endpoint: string): Observable<T> {
    return this.http.delete<ApiResponse<T>>(`${this.apiUrl}${endpoint}`).pipe(
      map(response => this.unwrapResponse(response))
    );
  }

  patch<T>(endpoint: string, body: any): Observable<T> {
    return this.http.patch<ApiResponse<T>>(`${this.apiUrl}${endpoint}`, body).pipe(
      map(response => this.unwrapResponse(response))
    );
  }

  private unwrapResponse<T>(response: ApiResponse<T>): T {
    // Handle null responses (e.g., from DELETE operations)
    if (response === null || response === undefined) {
      return response as T;
    }
    
    if (response.success && response.data !== undefined) {
      return response.data;
    }
    
    // Fallback for responses that aren't wrapped (like health endpoint)
    // or when data is undefined but response is successful
    return response as unknown as T;
  }
}
