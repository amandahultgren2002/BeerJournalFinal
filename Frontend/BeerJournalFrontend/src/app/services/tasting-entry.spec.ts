import { TestBed } from '@angular/core/testing';
import { TastingEntryService } from './tasting-entry-service';

describe('TastingEntryService', () => {
  let service: TastingEntryService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(TastingEntryService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});