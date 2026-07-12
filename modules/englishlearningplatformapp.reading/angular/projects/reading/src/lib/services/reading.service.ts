import { inject, Injectable } from '@angular/core';
import { RestService } from '@abp/ng.core';

@Injectable({
  providedIn: 'root',
})
export class ReadingService {
  apiName = 'Reading';

  private restService = inject(RestService);

  sample() {
    return this.restService.request<void, any>(
      { method: 'GET', url: '/api/reading/example' },
      { apiName: this.apiName }
    );
  }
}
