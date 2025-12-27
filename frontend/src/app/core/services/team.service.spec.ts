import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { TeamService, TeamDto, CreateTeamDto, UpdateTeamDto } from './team.service';
import { ApiService } from './api.service';
import { environment } from '@environments/environment';
import { VacationDto, VacationStatus, VacationType } from './vacation.service';

describe('TeamService', () => {
  let service: TeamService;
  let httpMock: HttpTestingController;

  const mockTeam: TeamDto = {
    id: '1',
    name: 'Engineering',
    description: 'Engineering team',
    createdAt: new Date(),
    updatedAt: new Date()
  };

  const mockVacation: VacationDto = {
    id: '1',
    userId: 'user-1',
    userName: 'Test User',
    startDate: new Date('2024-01-01'),
    endDate: new Date('2024-01-05'),
    status: VacationStatus.Approved,
    type: VacationType.Vacation,
    notes: 'Test vacation',
    createdAt: new Date(),
    updatedAt: new Date()
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [TeamService, ApiService]
    });
    service = TestBed.inject(TeamService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should get all teams', (done) => {
    const mockTeams = [mockTeam];
    
    service.getAllTeams().subscribe(teams => {
      expect(teams).toEqual(mockTeams);
      expect(teams.length).toBe(1);
      done();
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/v1/teams`);
    expect(req.request.method).toBe('GET');
    req.flush(mockTeams);
  });

  it('should get team by id', (done) => {
    service.getTeamById('1').subscribe(team => {
      expect(team).toEqual(mockTeam);
      done();
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/v1/teams/1`);
    expect(req.request.method).toBe('GET');
    req.flush(mockTeam);
  });

  it('should create team', (done) => {
    const createDto: CreateTeamDto = {
      name: 'Engineering',
      description: 'Engineering team'
    };

    service.createTeam(createDto).subscribe(team => {
      expect(team).toEqual(mockTeam);
      done();
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/v1/teams`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(createDto);
    req.flush(mockTeam);
  });

  it('should update team', (done) => {
    const updateDto: UpdateTeamDto = {
      name: 'Updated Engineering',
      description: 'Updated description'
    };

    service.updateTeam('1', updateDto).subscribe(team => {
      expect(team).toEqual(mockTeam);
      done();
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/v1/teams/1`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual(updateDto);
    req.flush(mockTeam);
  });

  it('should delete team', (done) => {
    service.deleteTeam('1').subscribe(() => {
      expect(true).toBe(true);
      done();
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/v1/teams/1`);
    expect(req.request.method).toBe('DELETE');
    req.flush(null);
  });

  it('should get team vacations without date filters', (done) => {
    const mockVacations = [mockVacation];

    service.getTeamVacations('1').subscribe(vacations => {
      expect(vacations).toEqual(mockVacations);
      expect(vacations.length).toBe(1);
      done();
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/v1/teams/1/vacations`);
    expect(req.request.method).toBe('GET');
    req.flush(mockVacations);
  });

  it('should get team vacations with date filters', (done) => {
    const mockVacations = [mockVacation];
    const startDate = new Date('2024-01-01');
    const endDate = new Date('2024-12-31');

    service.getTeamVacations('1', startDate, endDate).subscribe(vacations => {
      expect(vacations).toEqual(mockVacations);
      done();
    });

    const req = httpMock.expectOne((request) => {
      return request.url === `${environment.apiUrl}/v1/teams/1/vacations` &&
             request.params.get('startDate') === startDate.toISOString() &&
             request.params.get('endDate') === endDate.toISOString();
    });
    expect(req.request.method).toBe('GET');
    req.flush(mockVacations);
  });
});
