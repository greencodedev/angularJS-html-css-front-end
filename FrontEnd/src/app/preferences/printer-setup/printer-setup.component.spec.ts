import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { PrinterSetupComponent } from './printer-setup.component';

describe('PrinterSetupComponent', () => {
  let component: PrinterSetupComponent;
  let fixture: ComponentFixture<PrinterSetupComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ PrinterSetupComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(PrinterSetupComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
