import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { AvgCostLogComponent } from './avg-cost-log.component';

describe('AvgCostLogComponent', () => {
  let component: AvgCostLogComponent;
  let fixture: ComponentFixture<AvgCostLogComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ AvgCostLogComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(AvgCostLogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
