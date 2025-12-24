import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface UserDto {
  id: string;
  entraId: string;
  displayName: string;
  email: string;
  teamId?: string;
  isManager: boolean;
  createdAt: Date;
  updatedAt: Date;
}

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private endpoint = '/users';

  constructor(private apiService: ApiService) {}

  getCurrentUser(): Observable<UserDto> {
    return this.apiService.get<UserDto>(`${this.endpoint}/me`);
  }

  getUserById(id: string): Observable<UserDto> {
    return this.apiService.get<UserDto>(`${this.endpoint}/${id}`);
  }

  getAllUsers(): Observable<UserDto[]> {
    return this.apiService.get<UserDto[]>(this.endpoint);
  }

  getUsersByTeam(teamId: string): Observable<UserDto[]> {
    return this.apiService.get<UserDto[]>(`${this.endpoint}/team/${teamId}`);
  }

  registerUser(): Observable<UserDto> {
    return this.apiService.post<UserDto>(`${this.endpoint}/register`, {});
  }

  addUserToTeam(teamId: string): Observable<UserDto> {
    return this.apiService.post<UserDto>(`${this.endpoint}/team/${teamId}`, {});
  }

  removeUserFromTeam(): Observable<UserDto> {
    return this.apiService.delete<UserDto>(`${this.endpoint}/team`);
  }
}
