import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { PrinterSetupModalComponent } from './printer-setup-modal.component';

describe('PrinterSetupModalComponent', () => {
  let component: PrinterSetupModalComponent;
  let fixture: ComponentFixture<PrinterSetupModalComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ PrinterSetupModalComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(PrinterSetupModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
