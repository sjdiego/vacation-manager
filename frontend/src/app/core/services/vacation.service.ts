import { Injectable } from '@angular/core';
import { Observable, shareReplay, map } from 'rxjs';
import { ApiService } from './api.service';
import { HttpParams } from '@angular/common/http';

export enum VacationType {
  Vacation = 0,
  SickLeave = 1,
  PersonalDay = 2,
  CompensatoryTime = 3,
  Other = 4
}

export enum VacationStatus {
  Pending = 0,
  Approved = 1,
  Rejected = 2,
  Cancelled = 3
}

export interface VacationDto {
  id: string;
  userId: string;
  userName?: string;
  startDate: Date;
  endDate: Date;
  type: VacationType;
  status: VacationStatus;
  notes?: string;
  createdAt: Date;
  updatedAt: Date;
}

export interface CreateVacationDto {
  startDate: Date;
  endDate: Date;
  type: VacationType;
  notes?: string;
}

export interface UpdateVacationDto {
  startDate: Date;
  endDate: Date;
  type: VacationType;
  notes?: string;
}

export interface ApproveVacationDto {
  approved: boolean;
  rejectReason?: string;
}

@Injectable({
  providedIn: 'root'
})
export class VacationService {
  private endpoint = '/v1/vacations';
  private readonly CACHE_DURATION_MS = 5 * 60 * 1000; // 5 minutes
  private teamVacationsCache: Map<string, { data: Observable<VacationDto[]>, timestamp: number }> = new Map();

  constructor(private apiService: ApiService) {}

  private mapVacationType(type: string | number): VacationType {
    if (typeof type === 'number') return type;
    const typeMap: { [key: string]: VacationType } = {
      'Vacation': VacationType.Vacation,
      'SickLeave': VacationType.SickLeave,
      'PersonalDay': VacationType.PersonalDay,
      'CompensatoryTime': VacationType.CompensatoryTime,
      'Other': VacationType.Other
    };
    return typeMap[type] ?? VacationType.Other;
  }

  private mapVacationStatus(status: string | number): VacationStatus {
    if (typeof status === 'number') return status;
    const statusMap: { [key: string]: VacationStatus } = {
      'Pending': VacationStatus.Pending,
      'Approved': VacationStatus.Approved,
      'Rejected': VacationStatus.Rejected,
      'Cancelled': VacationStatus.Cancelled
    };
    return statusMap[status] ?? VacationStatus.Pending;
  }

  private normalizeVacation(vacation: any): VacationDto {
    return {
      ...vacation,
      type: this.mapVacationType(vacation.type),
      status: this.mapVacationStatus(vacation.status),
      startDate: new Date(vacation.startDate),
      endDate: new Date(vacation.endDate),
      createdAt: new Date(vacation.createdAt),
      updatedAt: new Date(vacation.updatedAt)
    };
  }

  getMyVacations(): Observable<VacationDto[]> {
    return this.apiService.get<any[]>(this.endpoint).pipe(
      map(vacations => vacations.map(v => this.normalizeVacation(v)))
    );
  }

  getVacationById(id: string): Observable<VacationDto> {
    return this.apiService.get<any>(`${this.endpoint}/${id}`).pipe(
      map(vacation => this.normalizeVacation(vacation))
    );
  }

  createVacation(vacation: CreateVacationDto): Observable<VacationDto> {
    return this.apiService.post<any>(this.endpoint, vacation).pipe(
      map(v => this.normalizeVacation(v))
    );
  }

  updateVacation(id: string, vacation: UpdateVacationDto): Observable<VacationDto> {
    return this.apiService.put<any>(`${this.endpoint}/${id}`, vacation).pipe(
      map(v => this.normalizeVacation(v))
    );
  }

  deleteVacation(id: string): Observable<void> {
    return this.apiService.delete<void>(`${this.endpoint}/${id}`);
  }

  getTeamVacations(startDate?: Date, endDate?: Date): Observable<VacationDto[]> {
    const cacheKey = this.getCacheKey(startDate, endDate);
    const now = Date.now();
    
    // Check if cached data is still valid
    const cached = this.teamVacationsCache.get(cacheKey);
    if (cached && (now - cached.timestamp) < this.CACHE_DURATION_MS) {
      return cached.data;
    }
    
    // Fetch fresh data and cache it
    const options: Record<string, string | string[]> = {};
    if (startDate) {
      options['startDate'] = startDate.toISOString();
    }
    if (endDate) {
      options['endDate'] = endDate.toISOString();
    }
    
    const data$ = this.apiService.get<any[]>(`${this.endpoint}/team`, options as any).pipe(
      map(vacations => vacations.map(v => this.normalizeVacation(v))),
      shareReplay(1)
    );
    
    this.teamVacationsCache.set(cacheKey, {
      data: data$,
      timestamp: now
    });
    
    return data$;
  }

  private getCacheKey(startDate?: Date, endDate?: Date): string {
    return `${startDate?.toISOString() || ''}_${endDate?.toISOString() || ''}`;
  }

  clearTeamVacationsCache(): void {
    this.teamVacationsCache.clear();
  }

  getTeamPendingVacations(): Observable<VacationDto[]> {
    return this.apiService.get<any[]>(`${this.endpoint}/team/pending`).pipe(
      map(vacations => vacations.map(v => this.normalizeVacation(v)))
    );
  }

  approveVacation(id: string, approved: boolean, rejectReason?: string): Observable<VacationDto> {
    const dto: ApproveVacationDto = { approved, rejectReason };
    return this.apiService.post<any>(`${this.endpoint}/${id}/approve`, dto).pipe(
      map(v => this.normalizeVacation(v))
    );
  }

  cancelVacation(id: string): Observable<VacationDto> {
    return this.apiService.post<any>(`${this.endpoint}/${id}/cancel`, {}).pipe(
      map(v => this.normalizeVacation(v))
    );
  }
}
