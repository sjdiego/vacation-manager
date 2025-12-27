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
