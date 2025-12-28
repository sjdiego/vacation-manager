import { TestBed } from '@angular/core/testing';
import { HttpErrorResponse } from '@angular/common/http';
import { ErrorHandlerService, ProblemDetails } from './error-handler.service';

describe('ErrorHandlerService', () => {
  let service: ErrorHandlerService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ErrorHandlerService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('handleError with Problem Details', () => {
    it('should extract detail from Problem Details', (done) => {
      const problemDetails: ProblemDetails = {
        type: 'https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1',
        title: 'Bad Request',
        status: 400,
        detail: 'Invalid input provided',
        instance: '/api/v1/vacations'
      };

      const error = new HttpErrorResponse({
        error: problemDetails,
        status: 400,
        statusText: 'Bad Request'
      });

      service.handleError(error).subscribe({
        error: (apiError) => {
          expect(apiError.status).toBe(400);
          expect(apiError.message).toBe('Invalid input provided');
          expect(apiError.details).toEqual(problemDetails);
          done();
        }
      });
    });

    it('should extract validation errors from Problem Details', (done) => {
      const problemDetails: ProblemDetails = {
        type: 'https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1',
        title: 'Bad Request',
        status: 400,
        detail: 'Validation failed',
        errors: {
          'startDate': ['Start date is required'],
          'endDate': ['End date must be after start date']
        }
      };

      const error = new HttpErrorResponse({
        error: problemDetails,
        status: 400,
        statusText: 'Bad Request'
      });

      service.handleError(error).subscribe({
        error: (apiError) => {
          expect(apiError.validationErrors).toBeDefined();
          expect(apiError.validationErrors!['startDate']).toEqual(['Start date is required']);
          expect(apiError.validationErrors!['endDate']).toEqual(['End date must be after start date']);
          done();
        }
      });
    });

    it('should use title if detail is not available', (done) => {
      const problemDetails: ProblemDetails = {
        type: 'https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4',
        title: 'Not Found',
        status: 404,
        instance: '/api/v1/vacations/123'
      };

      const error = new HttpErrorResponse({
        error: problemDetails,
        status: 404,
        statusText: 'Not Found'
      });

      service.handleError(error).subscribe({
        error: (apiError) => {
          expect(apiError.message).toBe('Not Found');
          done();
        }
      });
    });

    it('should handle 401 Unauthorized with Problem Details', (done) => {
      const problemDetails: ProblemDetails = {
        type: 'https://datatracker.ietf.org/doc/html/rfc7235#section-3.1',
        title: 'Unauthorized',
        status: 401,
        detail: 'You are not authorized to access this resource'
      };

      const error = new HttpErrorResponse({
        error: problemDetails,
        status: 401,
        statusText: 'Unauthorized'
      });

      service.handleError(error).subscribe({
        error: (apiError) => {
          expect(apiError.status).toBe(401);
          expect(apiError.message).toBe('You are not authorized to access this resource');
          done();
        }
      });
    });

    it('should handle 500 Internal Server Error with Problem Details', (done) => {
      const problemDetails: ProblemDetails = {
        type: 'https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1',
        title: 'An error occurred while processing your request',
        status: 500,
        detail: 'An unexpected error occurred',
        extensions: {
          traceId: 'trace-123'
        }
      };

      const error = new HttpErrorResponse({
        error: problemDetails,
        status: 500,
        statusText: 'Internal Server Error'
      });

      service.handleError(error).subscribe({
        error: (apiError) => {
          expect(apiError.status).toBe(500);
          expect(apiError.message).toBe('An unexpected error occurred');
          done();
        }
      });
    });
  });

  describe('handleError with legacy error format', () => {
    it('should handle legacy error format with message property', (done) => {
      const error = new HttpErrorResponse({
        error: { message: 'Legacy error message' },
        status: 400,
        statusText: 'Bad Request'
      });

      service.handleError(error).subscribe({
        error: (apiError) => {
          expect(apiError.message).toBe('Legacy error message');
          done();
        }
      });
    });

    it('should fallback to generic message for status code', (done) => {
      const error = new HttpErrorResponse({
        error: {},
        status: 400,
        statusText: 'Bad Request'
      });

      service.handleError(error).subscribe({
        error: (apiError) => {
          expect(apiError.message).toBe('Bad request. Please check your input.');
          done();
        }
      });
    });
  });

  describe('handleError with network errors', () => {
    it('should handle network connection error', (done) => {
      const error = new HttpErrorResponse({
        error: new ErrorEvent('Network error', { message: 'Network failed' }),
        status: 0,
        statusText: 'Unknown Error'
      });

      service.handleError(error).subscribe({
        error: (apiError) => {
          expect(apiError.status).toBe(0);
          expect(apiError.message).toBe('Network failed');
          done();
        }
      });
    });

    it('should handle offline status', (done) => {
      const error = new HttpErrorResponse({
        error: {},
        status: 0,
        statusText: 'Unknown Error'
      });

      service.handleError(error).subscribe({
        error: (apiError) => {
          expect(apiError.message).toBe('Unable to connect to the server. Please check your network connection.');
          done();
        }
      });
    });
  });

  describe('isProblemDetails detection', () => {
    it('should detect Problem Details with type property', (done) => {
      const problemDetails: ProblemDetails = {
        type: 'https://example.com/problem',
        status: 400
      };

      const error = new HttpErrorResponse({
        error: problemDetails,
        status: 400,
        statusText: 'Bad Request'
      });

      service.handleError(error).subscribe({
        error: (apiError) => {
          expect(apiError.details).toEqual(problemDetails);
          done();
        }
      });
    });

    it('should detect Problem Details with title property', (done) => {
      const problemDetails: ProblemDetails = {
        title: 'Error Title',
        status: 400
      };

      const error = new HttpErrorResponse({
        error: problemDetails,
        status: 400,
        statusText: 'Bad Request'
      });

      service.handleError(error).subscribe({
        error: (apiError) => {
          expect(apiError.message).toBe('Error Title');
          done();
        }
      });
    });

    it('should detect Problem Details with detail property', (done) => {
      const problemDetails: ProblemDetails = {
        detail: 'Error detail message',
        status: 400
      };

      const error = new HttpErrorResponse({
        error: problemDetails,
        status: 400,
        statusText: 'Bad Request'
      });

      service.handleError(error).subscribe({
        error: (apiError) => {
          expect(apiError.message).toBe('Error detail message');
          done();
        }
      });
    });
  });
});
