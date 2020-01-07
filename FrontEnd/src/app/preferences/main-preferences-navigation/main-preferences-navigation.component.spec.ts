import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { MainPreferencesNavigationComponent } from './main-preferences-navigation.component';

describe('MainPreferencesNavigationComponent', () => {
  let component: MainPreferencesNavigationComponent;
  let fixture: ComponentFixture<MainPreferencesNavigationComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ MainPreferencesNavigationComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(MainPreferencesNavigationComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
