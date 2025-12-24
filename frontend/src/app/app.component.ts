import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { AuthService, User } from '@app/core/services/auth.service';
import { ToastService } from '@app/core/services/toast.service';
import { UserService, UserDto } from '@app/core/services/user.service';

const BUILD_TIMESTAMP = new Date().toISOString();

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent implements OnInit {
  currentUser: User | null = null;
  currentUserDto: UserDto | null = null;
  toasts$ = this.toastService.toasts$;

  constructor(
    private authService: AuthService,
    private userService: UserService,
    private toastService: ToastService,
    private router: Router
  ) {
    console.log('App built at:', BUILD_TIMESTAMP);
  }

  ngOnInit(): void {
    this.authService.restoreUserState();
    
    const storedUser = this.authService.getStoredUser();
    if (storedUser) {
      this.currentUser = storedUser;
      this.loadUserDetails();
      if (this.router.url === '/login') {
        this.router.navigate(['/vacations']);
      }
    }

    this.authService.currentUser$.subscribe((user) => {
      this.currentUser = user;
      if (user) {
        this.loadUserDetails();
      }
    });
  }

  loadUserDetails(): void {
    this.userService.getCurrentUser().subscribe({
      next: (userDto) => {
        this.currentUserDto = userDto;
      },
      error: (error) => {
        console.error('Failed to load user details:', error);
      }
    });
  }

  logout(): void {
    this.authService.logout().then(() => {
      this.currentUserDto = null;
      this.toastService.success('Logged out successfully');
      this.router.navigate(['/login']);
    });
  }
}

