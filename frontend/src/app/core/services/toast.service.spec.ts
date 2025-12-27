import { TestBed } from '@angular/core/testing';
import { ToastService, ToastType } from './toast.service';

describe('ToastService', () => {
  let service: ToastService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ToastService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should add a toast', (done) => {
    service.success('Test message');
    
    service.toasts$.subscribe(toasts => {
      expect(toasts.length).toBe(1);
      expect(toasts[0].message).toBe('Test message');
      expect(toasts[0].type).toBe(ToastType.Success);
      done();
    });
  });

  it('should remove a toast by id', (done) => {
    service.info('Test');
    
    let toastId: string;
    service.toasts$.subscribe(toasts => {
      if (toasts.length === 1) {
        toastId = toasts[0].id;
        service.remove(toastId);
      } else if (toasts.length === 0 && toastId) {
        expect(toasts.length).toBe(0);
        done();
      }
    });
  });

  it('should clear all toasts', (done) => {
    service.success('Test 1');
    service.error('Test 2');
    service.clear();

    service.toasts$.subscribe(toasts => {
      expect(toasts.length).toBe(0);
      done();
    });
  });

  it('should auto-remove toast after duration', (done) => {
    jasmine.clock().install();
    
    service.info('Test', 1000);
    
    service.toasts$.subscribe(toasts => {
      if (toasts.length === 1) {
        jasmine.clock().tick(1001);
      } else {
        expect(toasts.length).toBe(0);
        jasmine.clock().uninstall();
        done();
      }
    });
  });
});
