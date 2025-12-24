# Frontend API Core Components

This document describes the core API integration layer for the vacation-manager frontend application.

## Architecture Overview

The frontend API layer is built with a layered approach:

1. **HTTP Client** (`HttpClient`) - Angular's built-in HTTP client
2. **Base API Service** (`ApiService`) - Generic HTTP wrapper
3. **Domain Services** - Type-safe service wrappers for each API domain
4. **Utility Services** - Cross-cutting concerns (error handling, loading, caching, etc.)
5. **Interceptors** - HTTP middleware for authentication and error handling

## Services

### ApiService (`api.service.ts`)

Base service providing generic HTTP operations with the following methods:

```typescript
get<T>(endpoint: string, params?: HttpParams | Record<string, string[]>): Observable<T>
post<T>(endpoint: string, body: any): Observable<T>
put<T>(endpoint: string, body: any): Observable<T>
delete<T>(endpoint: string): Observable<T>
patch<T>(endpoint: string, body: any): Observable<T>
```

All endpoints are automatically prefixed with the API base URL from environment configuration.

### Domain Services

#### UserService (`user.service.ts`)

Manages user-related API calls:

- `getCurrentUser()` - Get the current authenticated user
- `getUserById(id: string)` - Get a specific user by ID
- `getAllUsers()` - Get all users in the system
- `getUsersByTeam(teamId: string)` - Get users belonging to a team
- `registerUser()` - Register the current user (auto-creates from token claims)
- `addUserToTeam(teamId: string)` - Add current user to a team
- `removeUserFromTeam()` - Remove current user from their team

**Types:**
- `UserDto` - Complete user object with ID, email, display name, team ID, and timestamps

#### VacationService (`vacation.service.ts`)

Manages vacation-related API calls:

- `getMyVacations()` - Get all vacations for the current user
- `getVacationById(id: string)` - Get a specific vacation
- `createVacation(vacation: CreateVacationDto)` - Create a new vacation request
- `updateVacation(id: string, vacation: UpdateVacationDto)` - Update a vacation
- `deleteVacation(id: string)` - Delete a vacation
- `getTeamVacations(startDate?: Date, endDate?: Date)` - Get team's vacations in date range

**Types:**
- `VacationDto` - Complete vacation object
- `CreateVacationDto` - Request body for creating vacations
- `UpdateVacationDto` - Request body for updating vacations
- `VacationType` - Enum: Paid, Unpaid, Sick, Personal
- `VacationStatus` - Enum: Pending, Approved, Rejected

#### TeamService (`team.service.ts`)

Manages team-related API calls:

- `getAllTeams()` - Get all teams
- `getTeamById(id: string)` - Get a specific team
- `createTeam(team: CreateTeamDto)` - Create a new team
- `updateTeam(id: string, team: UpdateTeamDto)` - Update a team
- `deleteTeam(id: string)` - Delete a team
- `getTeamVacations(teamId: string, startDate?: Date, endDate?: Date)` - Get team's vacations

**Types:**
- `TeamDto` - Complete team object
- `CreateTeamDto` - Request body for creating teams
- `UpdateTeamDto` - Request body for updating teams

### Utility Services

#### ErrorHandlerService (`error-handler.service.ts`)

Centralized error handling with user-friendly messages:

```typescript
handleError(error: HttpErrorResponse): Observable<never>
```

Maps HTTP status codes to meaningful error messages:
- 400: Bad request with validation details
- 401: Unauthorized (requires re-authentication)
- 403: Forbidden (insufficient permissions)
- 404: Resource not found
- 409: Conflict (resource already exists)
- 422: Validation error
- 500+: Server errors

**Types:**
- `ApiError` - Structured error object with status, message, details, and timestamp

#### LoadingService (`loading.service.ts`)

Manages global loading state for UI indicators:

```typescript
show(): void              // Show loading indicator
hide(): void              // Hide loading indicator
isLoading(): boolean      // Get current loading state
loading$: Observable<boolean>  // Observable for reactive components
```

#### ToastService (`toast.service.ts`)

Manages toast notifications:

```typescript
show(message: string, type: ToastType, duration?: number): void
success(message: string, duration?: number): void
error(message: string, duration?: number): void
warning(message: string, duration?: number): void
info(message: string, duration?: number): void
remove(id: string): void
clear(): void
```

Toast types: `Success`, `Error`, `Warning`, `Info`

Default durations:
- Success/Info/Warning: 5 seconds
- Error: 7 seconds
- Custom duration can be specified

#### CacheService (`cache.service.ts`)

Request-level caching to reduce API calls:

```typescript
get<T>(key: string): T | null
set<T>(key: string, data: T, ttl?: number): void
cacheRequest<T>(key: string, request: Observable<T>): Observable<T>
invalidate(key: string): void
invalidatePattern(pattern: string): void  // Regex-based invalidation
clear(): void
```

Default TTL: 5 minutes

### Interceptors

#### AuthInterceptor (`auth.interceptor.ts`)

Automatically adds Bearer token to all outgoing requests:

```typescript
Authorization: Bearer {token}
```

Token is obtained from `AuthService.getToken()`.

#### ErrorInterceptor (`error.interceptor.ts`)

Handles HTTP errors globally:
- Routes 401 errors to login page
- Uses `ErrorHandlerService` for consistent error handling
- Allows error to propagate to components via RxJS `throwError`

## Usage Examples

### Basic Service Injection

```typescript
import { Component, OnInit } from '@angular/core';
import { VacationService, UserService, ToastService } from '@app/core/services';

@Component({
  selector: 'app-my-vacations',
  templateUrl: './my-vacations.component.html'
})
export class MyVacationsComponent implements OnInit {
  constructor(
    private vacationService: VacationService,
    private userService: UserService,
    private toastService: ToastService
  ) {}

  ngOnInit(): void {
    this.loadVacations();
  }

  loadVacations(): void {
    this.vacationService.getMyVacations().subscribe({
      next: (vacations) => {
        console.log('Vacations loaded:', vacations);
      },
      error: (error) => {
        this.toastService.error(error.message);
      }
    });
  }
}
```

### Using Cache Service

```typescript
constructor(private vacationService: VacationService, private cacheService: CacheService) {}

getTeamVacations(): void {
  const cacheKey = 'team-vacations-2025';
  this.cacheService.cacheRequest(
    cacheKey,
    this.vacationService.getTeamVacations()
  ).subscribe(vacations => {
    // Data is cached for subsequent calls within 5 minutes
  });
}
```

### Error Handling

Errors are automatically caught by the error interceptor and can be handled in components:

```typescript
this.userService.getCurrentUser().subscribe({
  next: (user) => console.log('User:', user),
  error: (apiError: ApiError) => {
    console.error(`Error ${apiError.status}: ${apiError.message}`);
  }
});
```

## Configuration

API base URL is configured in environment files:

**environment.ts** (development):
```typescript
export const environment = {
  apiUrl: 'http://localhost:5000/api',
  // ...
};
```

**environment.prod.ts** (production):
```typescript
export const environment = {
  apiUrl: 'https://api.example.com/api',
  // ...
};
```

## Authentication Flow

1. User authenticates via MSAL (configured in `AuthService`)
2. Token is stored in browser storage (implementation in `AuthService`)
3. `AuthInterceptor` automatically adds token to all requests
4. If token is invalid (401), user is redirected to login
5. `ErrorInterceptor` handles the redirect

## Best Practices

1. **Always use typed DTOs** - Leverage TypeScript for compile-time safety
2. **Use Observables** - Don't convert to Promises unless necessary
3. **Handle errors gracefully** - Always provide error handlers
4. **Cache when appropriate** - Use `CacheService` for read-only data
5. **Show feedback** - Use `ToastService` for user notifications
6. **Manage loading state** - Use `LoadingService` for UI feedback
7. **Clean up subscriptions** - Use `async` pipe or `takeUntilDestroyed`

## Testing

Services can be tested using Angular's testing utilities:

```typescript
import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { VacationService } from './vacation.service';

describe('VacationService', () => {
  let service: VacationService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [VacationService]
    });

    service = TestBed.inject(VacationService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should fetch user vacations', () => {
    service.getMyVacations().subscribe((vacations) => {
      expect(vacations.length).toBe(2);
    });

    const req = httpMock.expectOne('/api/vacations');
    expect(req.request.method).toBe('GET');
    req.flush([/* mock data */]);
  });
});
```

## Troubleshooting

### CORS Issues
Ensure backend has CORS enabled for your frontend origin. Check `Program.cs` in backend for CORS configuration.

### 401 Unauthorized
- Verify token is being obtained from `AuthService`
- Check that token hasn't expired
- Ensure `AuthInterceptor` is properly configured in `app.config.ts`

### Compilation Errors
- Run `npm install` to ensure all dependencies are installed
- Check TypeScript version compatibility (5.5 - 5.8)
- Run `npm run lint` to identify style issues

## Future Enhancements

- [ ] Request/response logging service
- [ ] Retry logic for failed requests
- [ ] Optimistic updates for better UX
- [ ] WebSocket support for real-time updates
- [ ] Advanced caching strategies (LRU, persistence)
