import { TestBed } from '@angular/core/testing';
import { SwUpdate, VersionEvent } from '@angular/service-worker';
import { Subject } from 'rxjs';
import { describe, expect, it } from 'vitest';
import { PwaUpdateService } from './pwa-update.service';

describe('PwaUpdateService', () => {
  it('requires an explicit user action after a new version becomes ready', () => {
    const versionUpdates = new Subject<VersionEvent>();
    TestBed.configureTestingModule({
      providers: [
        PwaUpdateService,
        {
          provide: SwUpdate,
          useValue: {
            isEnabled: true,
            versionUpdates,
            activateUpdate: () => Promise.resolve(true),
          },
        },
      ],
    });

    const service = TestBed.inject(PwaUpdateService);
    expect(service.updateAvailable()).toBe(false);

    versionUpdates.next({
      type: 'VERSION_READY',
      currentVersion: { hash: 'current' },
      latestVersion: { hash: 'latest' },
    });

    expect(service.updateAvailable()).toBe(true);
    service.dismiss();
    expect(service.updateAvailable()).toBe(false);
  });
});
