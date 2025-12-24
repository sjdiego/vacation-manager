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
  private endpoint = '/vacations';
  private readonly CACHE_DURATION_MS = 5 * 60 * 1000; // 5 minutes
  private teamVacationsCache: Map<string, { data: Observable<VacationDto[]>, timestamp: number }> = new Map();

  constructor(private apiService: ApiService) {}

  getMyVacations(): Observable<VacationDto[]> {
    return this.apiService.get<VacationDto[]>(this.endpoint);
  }

  getVacationById(id: string): Observable<VacationDto> {
    return this.apiService.get<VacationDto>(`${this.endpoint}/${id}`);
  }

  createVacation(vacation: CreateVacationDto): Observable<VacationDto> {
    return this.apiService.post<VacationDto>(this.endpoint, vacation);
  }

  updateVacation(id: string, vacation: UpdateVacationDto): Observable<VacationDto> {
    return this.apiService.put<VacationDto>(`${this.endpoint}/${id}`, vacation);
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
    
    const data$ = this.apiService.get<VacationDto[]>(`${this.endpoint}/team`, options as any).pipe(
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
    return this.apiService.get<VacationDto[]>(`${this.endpoint}/team/pending`);
  }

  approveVacation(id: string, approved: boolean, rejectReason?: string): Observable<VacationDto> {
    const dto: ApproveVacationDto = { approved, rejectReason };
    return this.apiService.post<VacationDto>(`${this.endpoint}/${id}/approve`, dto);
  }
}
