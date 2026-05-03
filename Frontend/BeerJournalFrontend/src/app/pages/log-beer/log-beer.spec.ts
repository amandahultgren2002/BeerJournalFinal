import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LogBeer } from './log-beer';

describe('LogBeer', () => {
  let component: LogBeer;
  let fixture: ComponentFixture<LogBeer>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LogBeer],
    }).compileComponents();

    fixture = TestBed.createComponent(LogBeer);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
