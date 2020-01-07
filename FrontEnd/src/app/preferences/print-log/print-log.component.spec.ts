import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { PrintLogComponent } from './print-log.component';

describe('PrintLogComponent', () => {
  let component: PrintLogComponent;
  let fixture: ComponentFixture<PrintLogComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ PrintLogComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(PrintLogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
