import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RegisterWithThirdPartyComponent } from './register-with-third-party.component';

describe('RegisterWithThirdPartyComponent', () => {
  let component: RegisterWithThirdPartyComponent;
  let fixture: ComponentFixture<RegisterWithThirdPartyComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ RegisterWithThirdPartyComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(RegisterWithThirdPartyComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
