import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { UserService, UserDto } from './user.service';
import { ApiService } from './api.service';
import { environment } from '@environments/environment';

describe('UserService', () => {
  let service: UserService;
  let httpMock: HttpTestingController;
  let apiService: ApiService;

  const mockUser: UserDto = {
    id: '1',
    entraId: 'entra-1',
    displayName: 'Test User',
    email: 'test@example.com',
    teamId: 'team-1',
    isManager: false,
    createdAt: new Date(),
    updatedAt: new Date()
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [UserService, ApiService]
    });
    service = TestBed.inject(UserService);
    apiService = TestBed.inject(ApiService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
    service.clearCache();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should get current user', (done) => {
    service.getCurrentUser().subscribe(user => {
      expect(user).toEqual(mockUser);
      done();
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/users/me`);
    expect(req.request.method).toBe('GET');
    req.flush(mockUser);
  });

  it('should cache current user after first call', (done) => {
    // First call - makes HTTP request
    service.getCurrentUser().subscribe(user => {
      expect(user).toEqual(mockUser);
      
      // Second call - uses cached value
      service.getCurrentUser().subscribe(cachedUser => {
        expect(cachedUser).toEqual(mockUser);
        done();
      });
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/users/me`);
    req.flush(mockUser);
  });

  it('should invalidate cache after TTL expires', (done) => {
    const realDateNow = Date.now.bind(Date);
    let currentTime = realDateNow();
    
    spyOn(Date, 'now').and.callFake(() => currentTime);
    
    service.getCurrentUser().subscribe(user => {
      expect(user).toEqual(mockUser);
    });

    const req1 = httpMock.expectOne(`${environment.apiUrl}/users/me`);
    req1.flush(mockUser);

    // Fast forward 6 minutes (past TTL)
    currentTime += 6 * 60 * 1000;

    service.getCurrentUser().subscribe(user => {
      expect(user).toEqual(mockUser);
      done();
    });

    const req2 = httpMock.expectOne(`${environment.apiUrl}/users/me`);
    req2.flush(mockUser);
  });

  it('should clear cache when clearCache is called', (done) => {
    service.getCurrentUser().subscribe(user => {
      expect(user).toEqual(mockUser);
    });

    const req1 = httpMock.expectOne(`${environment.apiUrl}/users/me`);
    req1.flush(mockUser);

    service.clearCache();

    service.getCurrentUser().subscribe(user => {
      expect(user).toEqual(mockUser);
      done();
    });

    const req2 = httpMock.expectOne(`${environment.apiUrl}/users/me`);
    req2.flush(mockUser);
  });

  it('should clear cache when user is added to team', (done) => {
    const teamId = 'team-1';
    
    service.getCurrentUser().subscribe(() => {});
    const req1 = httpMock.expectOne(`${environment.apiUrl}/users/me`);
    req1.flush(mockUser);

    service.addUserToTeam(teamId).subscribe(() => {
      expect(service['cacheTimestamp']).toBe(0);
      done();
    });

    const req2 = httpMock.expectOne(`${environment.apiUrl}/users/team/${teamId}`);
    req2.flush(mockUser);
  });

  it('should clear cache when user is removed from team', (done) => {
    service.getCurrentUser().subscribe(() => {});
    const req1 = httpMock.expectOne(`${environment.apiUrl}/users/me`);
    req1.flush(mockUser);

    service.removeUserFromTeam().subscribe(() => {
      expect(service['cacheTimestamp']).toBe(0);
      done();
    });

    const req2 = httpMock.expectOne(`${environment.apiUrl}/users/team`);
    req2.flush(mockUser);
  });

  it('should get all users', (done) => {
    const mockUsers = [mockUser];
    
    service.getAllUsers().subscribe(users => {
      expect(users).toEqual(mockUsers);
      expect(users.length).toBe(1);
      done();
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/users`);
    expect(req.request.method).toBe('GET');
    req.flush(mockUsers);
  });

  it('should check if user is manager', () => {
    service['currentUserSubject'].next({ ...mockUser, isManager: true });
    expect(service.isUserManager()).toBe(true);

    service['currentUserSubject'].next({ ...mockUser, isManager: false });
    expect(service.isUserManager()).toBe(false);
  });

  it('should check if user is in team', () => {
    service['currentUserSubject'].next({ ...mockUser, teamId: 'team-1' });
    expect(service.isUserInTeam()).toBe(true);

    service['currentUserSubject'].next({ ...mockUser, teamId: undefined });
    expect(service.isUserInTeam()).toBe(false);
  });
});
