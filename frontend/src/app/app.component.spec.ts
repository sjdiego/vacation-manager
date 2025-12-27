import { TestBed } from '@angular/core/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { of } from 'rxjs';
import { AppComponent } from './app.component';
import { AuthService } from '@app/core/services/auth.service';
import { ToastService } from '@app/core/services/toast.service';
import { UserService } from '@app/core/services/user.service';

describe('AppComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AppComponent, RouterTestingModule],
    })
      .overrideProvider(AuthService, {
        useValue: {
          currentUser$: of(null),
          restoreUserState: () => undefined,
          getStoredUser: () => null,
          logout: () => Promise.resolve(),
        },
      })
      .overrideProvider(UserService, {
        useValue: {
          getCurrentUser: () => of(null),
        },
      })
      .overrideProvider(ToastService, {
        useValue: {
          toasts$: of([]),
          success: () => undefined,
        },
      })
      .compileComponents();
  });

  it('should create', () => {
    const fixture = TestBed.createComponent(AppComponent);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });
});
