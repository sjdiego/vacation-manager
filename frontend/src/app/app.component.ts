import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { AuthService, User } from '@app/core/services/auth.service';
import { ToastService } from '@app/core/services/toast.service';
import { UserService, UserDto } from '@app/core/services/user.service';


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
  }

  ngOnInit(): void {
    this.authService.restoreUserState();

    const storedUser = this.authService.getStoredUser();
    if (storedUser) {
      this.currentUser = storedUser;
      if (this.router.url === '/login') {
        this.router.navigate(['/vacations']);
      }
    }

    // Subscribe to auth changes
    this.authService.currentUser$.subscribe((user) => {
      this.currentUser = user;
    });

    // Load user details once and subscribe to updates
    this.userService.getCurrentUser().subscribe({
      next: (userDto) => {
        this.currentUserDto = userDto;
      },
      error: (error) => {
        console.error('Failed to load user details:', error);
      }
    });

    // Subscribe to user details updates (e.g., team changes)
    this.userService.currentUser$.subscribe((user) => {
      if (user) {
        this.currentUserDto = user;
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
