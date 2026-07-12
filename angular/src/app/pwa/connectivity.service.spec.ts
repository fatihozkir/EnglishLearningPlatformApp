import { TestBed } from '@angular/core/testing';
import { beforeEach, describe, expect, it } from 'vitest';
import { ConnectivityService } from './connectivity.service';

describe('ConnectivityService', () => {
  beforeEach(() => {
    Object.defineProperty(window.navigator, 'onLine', { configurable: true, value: true });
    TestBed.configureTestingModule({ providers: [ConnectivityService] });
  });

  it('tracks offline and reconnect browser events', () => {
    const service = TestBed.inject(ConnectivityService);
    expect(service.online()).toBe(true);

    window.dispatchEvent(new Event('offline'));
    expect(service.online()).toBe(false);

    window.dispatchEvent(new Event('online'));
    expect(service.online()).toBe(true);
  });
});
