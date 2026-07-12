import { DOCUMENT, isPlatformBrowser } from '@angular/common';
import { Injectable, PLATFORM_ID, inject, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class ConnectivityService {
  private readonly platformId = inject(PLATFORM_ID);
  private readonly document = inject(DOCUMENT);
  private readonly browser = isPlatformBrowser(this.platformId);

  readonly online = signal(this.browser ? this.document.defaultView?.navigator.onLine !== false : true);

  constructor() {
    if (!this.browser) return;

    this.document.defaultView?.addEventListener('online', () => this.online.set(true));
    this.document.defaultView?.addEventListener('offline', () => this.online.set(false));
  }
}
