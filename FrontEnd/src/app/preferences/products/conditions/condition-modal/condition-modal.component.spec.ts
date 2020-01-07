import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ConditionModalComponent } from './condition-modal.component';

describe('ConditionModalComponent', () => {
  let component: ConditionModalComponent;
  let fixture: ComponentFixture<ConditionModalComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ConditionModalComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ConditionModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
