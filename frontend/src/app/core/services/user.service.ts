import { Injectable } from '@angular/core';
import { Observable, BehaviorSubject, of } from 'rxjs';
import { tap, shareReplay } from 'rxjs/operators';
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
  private endpoint = '/v1/users';
  private currentUserSubject = new BehaviorSubject<UserDto | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();
  private currentUserCache$: Observable<UserDto> | null = null;
  private cacheTimestamp: number = 0;
  private readonly CACHE_TTL_MS = 5 * 60 * 1000; // 5 minutes

  constructor(private apiService: ApiService) {}

  getCurrentUser(): Observable<UserDto> {
    const cachedUser = this.currentUserSubject.value;
    const now = Date.now();
    const isCacheValid = cachedUser && (now - this.cacheTimestamp) < this.CACHE_TTL_MS;
    
    if (isCacheValid) {
      return of(cachedUser);
    }

    if (!this.currentUserCache$) {
      this.currentUserCache$ = this.apiService.get<UserDto>(`${this.endpoint}/me`).pipe(
        tap(user => {
          this.currentUserSubject.next(user);
          this.cacheTimestamp = Date.now();
        }),
        shareReplay(1)
      );
    }

    return this.currentUserCache$;
  }

  clearCache(): void {
    this.currentUserCache$ = null;
    this.currentUserSubject.next(null);
    this.cacheTimestamp = 0;
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
    return this.apiService.post<UserDto>(`${this.endpoint}/team/${teamId}`, {}).pipe(
      tap(() => this.clearCache())
    );
  }

  removeUserFromTeam(): Observable<UserDto> {
    return this.apiService.delete<UserDto>(`${this.endpoint}/team`).pipe(
      tap(() => this.clearCache())
    );
  }

  assignUserToTeam(userId: string, teamId: string): Observable<UserDto> {
    return this.apiService.put<UserDto>(`${this.endpoint}/${userId}/team/${teamId}`, {}).pipe(
      tap(() => {
        // Clear cache if modifying current user
        const currentUser = this.currentUserSubject.value;
        if (currentUser && currentUser.id === userId) {
          this.clearCache();
        }
      })
    );
  }

  removeUserFromTeamAsManager(userId: string): Observable<UserDto> {
    return this.apiService.delete<UserDto>(`${this.endpoint}/${userId}/team`).pipe(
      tap(() => {
        // Clear cache if modifying current user
        const currentUser = this.currentUserSubject.value;
        if (currentUser && currentUser.id === userId) {
          this.clearCache();
        }
      })
    );
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
