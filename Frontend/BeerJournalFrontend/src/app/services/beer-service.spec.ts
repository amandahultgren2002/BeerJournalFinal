import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { BeerService } from './beer-service';

describe('BeerService', () => {
  let service: BeerService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });
    service = TestBed.inject(BeerService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});