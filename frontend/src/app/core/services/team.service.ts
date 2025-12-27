import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { HttpParams } from '@angular/common/http';
import { VacationDto } from './vacation.service';

export interface TeamDto {
  id: string;
  name: string;
  description?: string;
  createdAt: Date;
  updatedAt: Date;
}

export interface CreateTeamDto {
  name: string;
  description?: string;
}

export interface UpdateTeamDto {
  name?: string;
  description?: string;
}

@Injectable({
  providedIn: 'root'
})
export class TeamService {
  private endpoint = '/v1/teams';

  constructor(private apiService: ApiService) {}

  getAllTeams(): Observable<TeamDto[]> {
    return this.apiService.get<TeamDto[]>(this.endpoint);
  }

  getTeamById(id: string): Observable<TeamDto> {
    return this.apiService.get<TeamDto>(`${this.endpoint}/${id}`);
  }

  createTeam(team: CreateTeamDto): Observable<TeamDto> {
    return this.apiService.post<TeamDto>(this.endpoint, team);
  }

  updateTeam(id: string, team: UpdateTeamDto): Observable<TeamDto> {
    return this.apiService.put<TeamDto>(`${this.endpoint}/${id}`, team);
  }

  deleteTeam(id: string): Observable<void> {
    return this.apiService.delete<void>(`${this.endpoint}/${id}`);
  }

  getTeamVacations(teamId: string, startDate?: Date, endDate?: Date): Observable<VacationDto[]> {
    let params = new HttpParams();
    if (startDate) {
      params = params.set('startDate', startDate.toISOString());
    }
    if (endDate) {
      params = params.set('endDate', endDate.toISOString());
    }
    return this.apiService.get<VacationDto[]>(`${this.endpoint}/${teamId}/vacations`, params);
  }
}
