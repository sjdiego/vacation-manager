import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { VacationService, VacationDto, CreateVacationDto, VacationType, VacationStatus } from './vacation.service';
import { ApiService } from './api.service';
import { environment } from '@environments/environment';

describe('VacationService', () => {
  let service: VacationService;
  let httpMock: HttpTestingController;

  const mockVacation: VacationDto = {
    id: '1',
    userId: 'user-1',
    userName: 'Test User',
    startDate: new Date('2024-01-01'),
    endDate: new Date('2024-01-05'),
    type: VacationType.Vacation,
    status: VacationStatus.Pending,
    notes: 'Test vacation',
    createdAt: new Date(),
    updatedAt: new Date()
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [VacationService, ApiService]
    });
    service = TestBed.inject(VacationService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should get my vacations', (done) => {
    const mockVacations = [mockVacation];

    service.getMyVacations().subscribe(vacations => {
      expect(vacations).toEqual(mockVacations);
      expect(vacations.length).toBe(1);
      done();
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/vacations`);
    expect(req.request.method).toBe('GET');
    req.flush(mockVacations);
  });

  it('should get vacation by id', (done) => {
    service.getVacationById('1').subscribe(vacation => {
      expect(vacation).toEqual(mockVacation);
      done();
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/vacations/1`);
    expect(req.request.method).toBe('GET');
    req.flush(mockVacation);
  });

  it('should create vacation', (done) => {
    const createDto: CreateVacationDto = {
      startDate: new Date('2024-01-01'),
      endDate: new Date('2024-01-05'),
      type: VacationType.Vacation,
      notes: 'Test vacation'
    };

    service.createVacation(createDto).subscribe(vacation => {
      expect(vacation).toEqual(mockVacation);
      done();
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/vacations`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(createDto);
    req.flush(mockVacation);
  });

  it('should delete vacation', (done) => {
    service.deleteVacation('1').subscribe(() => {
      expect(true).toBe(true);
      done();
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/vacations/1`);
    expect(req.request.method).toBe('DELETE');
    req.flush(null);
  });
});
