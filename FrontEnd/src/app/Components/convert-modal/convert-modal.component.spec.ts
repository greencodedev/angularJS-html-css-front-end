import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ConvertModalComponent } from './convert-modal.component';

describe('ConvertModalComponent', () => {
  let component: ConvertModalComponent;
  let fixture: ComponentFixture<ConvertModalComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ ConvertModalComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ConvertModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
