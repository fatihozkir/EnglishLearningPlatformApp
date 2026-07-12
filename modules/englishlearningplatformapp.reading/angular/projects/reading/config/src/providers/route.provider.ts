import { eLayoutType, RoutesService } from '@abp/ng.core';
import {
  EnvironmentProviders,
  inject,
  makeEnvironmentProviders,
  provideAppInitializer,
} from '@angular/core';
import { eReadingRouteNames } from '../enums/route-names';

export const READING_ROUTE_PROVIDERS = [
  provideAppInitializer(() => {
    configureRoutes();
  }),
];

export function configureRoutes() {
  const routesService = inject(RoutesService);
  routesService.add([
    {
      path: '/reading',
      name: eReadingRouteNames.Reading,
      iconClass: 'fas fa-book',
      layout: eLayoutType.application,
      order: 3,
    },
  ]);
}

const READING_PROVIDERS: EnvironmentProviders[] = [...READING_ROUTE_PROVIDERS];

export function provideReading() {
  return makeEnvironmentProviders(READING_PROVIDERS);
}
