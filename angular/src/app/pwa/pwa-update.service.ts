import { Injectable, inject, signal } from '@angular/core';
import { SwUpdate, VersionReadyEvent } from '@angular/service-worker';
import { filter } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class PwaUpdateService {
  private readonly updates = inject(SwUpdate);

  readonly updateAvailable = signal(false);
  readonly activating = signal(false);

  constructor() {
    if (!this.updates.isEnabled) return;

    this.updates.versionUpdates
      .pipe(filter((event): event is VersionReadyEvent => event.type === 'VERSION_READY'))
      .subscribe(() => this.updateAvailable.set(true));
  }

  dismiss(): void {
    this.updateAvailable.set(false);
  }

  async activate(): Promise<void> {
    if (this.activating()) return;

    this.activating.set(true);
    try {
      await this.updates.activateUpdate();
      document.location.reload();
    } finally {
      this.activating.set(false);
    }
  }
}
