import { Injectable } from '@angular/core';
import { Observable, BehaviorSubject } from 'rxjs';
import { tap } from 'rxjs/operators';
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
  private currentUserSubject = new BehaviorSubject<UserDto | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(private apiService: ApiService) {}

  getCurrentUser(): Observable<UserDto> {
    return this.apiService.get<UserDto>(`${this.endpoint}/me`).pipe(
      tap(user => this.currentUserSubject.next(user))
    );
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

  addUserToTeam(teamId: string): Observable<UserDto> {
    return this.apiService.post<UserDto>(`${this.endpoint}/team/${teamId}`, {});
  }

  removeUserFromTeam(): Observable<UserDto> {
    return this.apiService.delete<UserDto>(`${this.endpoint}/team`);
  }

  assignUserToTeam(userId: string, teamId: string): Observable<UserDto> {
    return this.apiService.put<UserDto>(`${this.endpoint}/${userId}/team/${teamId}`, {});
  }

  removeUserFromTeamAsManager(userId: string): Observable<UserDto> {
    return this.apiService.delete<UserDto>(`${this.endpoint}/${userId}/team`);
  }

  isUserInTeam(): boolean {
    return !!this.currentUserSubject.value?.teamId;
  }

  isUserManager(): boolean {
    return !!this.currentUserSubject.value?.isManager;
  }

  getCurrentUserValue(): UserDto | null {
    return this.currentUserSubject.value;
  }
}
