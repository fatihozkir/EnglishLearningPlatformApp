import { Component, inject } from '@angular/core';
import { ConnectivityService } from './connectivity.service';
import { PwaUpdateService } from './pwa-update.service';

@Component({
  selector: 'app-pwa-status-banner',
  standalone: true,
  template: `
    @if (!connectivity.online()) {
      <aside class="pwa-notice pwa-notice--offline" role="status" aria-live="polite">
        <strong>You are offline.</strong>
        The app shell is available, but sign-in, learning activities, saving, submission, results, and administration require a connection.
      </aside>
    }
    @if (updates.updateAvailable()) {
      <aside class="pwa-notice pwa-notice--update" role="status" aria-live="polite">
        <span><strong>An update is ready.</strong> Reload only when you have no unsaved work.</span>
        <span class="pwa-notice__actions">
          <button type="button" class="btn btn-sm btn-primary" [disabled]="updates.activating()" (click)="updates.activate()">
            {{ updates.activating() ? 'Updating…' : 'Reload now' }}
          </button>
          <button type="button" class="btn btn-sm btn-outline-secondary" (click)="updates.dismiss()">Later</button>
        </span>
      </aside>
    }
  `,
  styles: `
    .pwa-notice { position: relative; z-index: 1100; display: flex; align-items: center; justify-content: center; gap: .75rem; padding: .65rem 1rem; font-size: .9rem; text-align: center; }
    .pwa-notice--offline { color: #3d2b00; background: #fff3cd; border-bottom: 1px solid #ffecb5; }
    .pwa-notice--update { color: #052c65; background: #cfe2ff; border-bottom: 1px solid #9ec5fe; }
    .pwa-notice__actions { display: inline-flex; gap: .5rem; flex-wrap: wrap; }
    @media (max-width: 576px) { .pwa-notice { align-items: stretch; flex-direction: column; } .pwa-notice__actions { justify-content: center; } }
  `,
})
export class PwaStatusBannerComponent {
  readonly connectivity = inject(ConnectivityService);
  readonly updates = inject(PwaUpdateService);
}
