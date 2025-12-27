import { TestBed } from '@angular/core/testing';
import { CacheService } from './cache.service';
import { of } from 'rxjs';

describe('CacheService', () => {
  let service: CacheService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [CacheService]
    });
    service = TestBed.inject(CacheService);
  });

  afterEach(() => {
    service.clear();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should set and get cache value', () => {
    const key = 'test-key';
    const value = { data: 'test data' };

    service.set(key, value);
    const result = service.get(key);

    expect(result).toEqual(value);
  });

  it('should return null for non-existent key', () => {
    const result = service.get('non-existent');
    expect(result).toBeNull();
  });

  it('should invalidate cache after TTL expires', () => {
    const realDateNow = Date.now.bind(Date);
    let currentTime = realDateNow();
    
    spyOn(Date, 'now').and.callFake(() => currentTime);

    const key = 'test-key';
    const value = { data: 'test data' };

    service.set(key, value);
    expect(service.get(key)).toEqual(value);

    // Fast forward 6 minutes (past default TTL of 5 minutes)
    currentTime += 6 * 60 * 1000;

    const result = service.get(key);
    expect(result).toBeNull();
  });

  it('should cache observable requests', (done) => {
    const key = 'test-observable';
    const testData = { value: 'test' };
    const observable$ = of(testData);

    // First call - should execute observable and cache
    service.cacheRequest(key, observable$).subscribe(result => {
      expect(result).toEqual(testData);
      
      // Second call - should return cached value
      service.cacheRequest(key, of({ value: 'different' })).subscribe(cachedResult => {
        expect(cachedResult).toEqual(testData);
        done();
      });
    });
  });

  it('should invalidate specific cache key', () => {
    const key1 = 'test-key-1';
    const key2 = 'test-key-2';
    const value1 = { data: 'test 1' };
    const value2 = { data: 'test 2' };

    service.set(key1, value1);
    service.set(key2, value2);

    service.invalidate(key1);

    expect(service.get(key1)).toBeNull();
    expect(service.get(key2)).toEqual(value2);
  });

  it('should invalidate cache by pattern', () => {
    service.set('user-1', { id: 1 });
    service.set('user-2', { id: 2 });
    service.set('team-1', { id: 1 });

    service.invalidatePattern('^user-');

    expect(service.get('user-1')).toBeNull();
    expect(service.get('user-2')).toBeNull();
    expect(service.get('team-1')).toEqual({ id: 1 });
  });

  it('should clear all cache', () => {
    service.set('key1', { data: 'value1' });
    service.set('key2', { data: 'value2' });
    service.set('key3', { data: 'value3' });

    service.clear();

    expect(service.get('key1')).toBeNull();
    expect(service.get('key2')).toBeNull();
    expect(service.get('key3')).toBeNull();
  });

  it('should handle multiple gets without side effects', () => {
    const key = 'test-key';
    const value = { data: 'test' };

    service.set(key, value);

    const result1 = service.get(key);
    const result2 = service.get(key);
    const result3 = service.get(key);

    expect(result1).toEqual(value);
    expect(result2).toEqual(value);
    expect(result3).toEqual(value);
  });

  it('should handle complex objects in cache', () => {
    const key = 'complex-object';
    const complexValue = {
      id: '123',
      name: 'Test',
      nested: {
        array: [1, 2, 3],
        object: { prop: 'value' }
      },
      date: new Date('2024-01-01')
    };

    service.set(key, complexValue);
    const result = service.get(key);

    expect(result).toEqual(complexValue);
  });
});
