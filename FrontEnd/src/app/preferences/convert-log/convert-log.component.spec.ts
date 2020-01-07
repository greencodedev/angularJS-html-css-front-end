import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ConvertLogComponent } from './convert-log.component';

describe('ConvertLogComponent', () => {
  let component: ConvertLogComponent;
  let fixture: ComponentFixture<ConvertLogComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ConvertLogComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ConvertLogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
