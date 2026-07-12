import { Component, inject } from '@angular/core';
import { ReadingService } from '../services/reading.service';

@Component({
  selector: 'lib-reading',
  template: ` <p>reading works!</p> `,
})
export class ReadingComponent {
  protected readonly service = inject(ReadingService);

  constructor() {
    this.service.sample().subscribe(console.log);
  }
}
