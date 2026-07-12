import { RouterOutletComponent } from '@abp/ng.core';
import { Routes } from '@angular/router';

export const READING_ROUTES: Routes = [
  {
    path: '',
    pathMatch: 'full',
    component: RouterOutletComponent,
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./components/reading.component').then(c => c.ReadingComponent),
      },
    ],
  },
];
