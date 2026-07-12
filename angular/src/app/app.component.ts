import { Component } from '@angular/core';
import { DynamicLayoutComponent } from '@abp/ng.core';
import { LoaderBarComponent } from '@abp/ng.theme.shared';
import { PwaStatusBannerComponent } from './pwa/pwa-status-banner.component';

@Component({
  selector: 'app-root',
  template: `
    <abp-loader-bar />
    <app-pwa-status-banner />
    <abp-dynamic-layout />
  `,
  imports: [LoaderBarComponent, PwaStatusBannerComponent, DynamicLayoutComponent],
})
export class AppComponent {}
