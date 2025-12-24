import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { AuthService, User } from '@app/core/services/auth.service';
import { ToastService } from '@app/core/services/toast.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent implements OnInit {
  currentUser: User | null = null;
  toasts$ = this.toastService.toasts$;

  constructor(
    private authService: AuthService,
    private toastService: ToastService,
    private router: Router
  ) {}

  ngOnInit(): void {
    // Restore user state on init
    this.authService.restoreUserState();
    
    // Load stored user
    const storedUser = this.authService.getStoredUser();
    if (storedUser) {
      this.currentUser = storedUser;
      // If on login page and already authenticated, navigate to home
      if (this.router.url === '/login') {
        this.router.navigate(['/vacations']);
      }
    }

    // Subscribe to user changes
    this.authService.currentUser$.subscribe((user) => {
      this.currentUser = user;
    });
  }

  logout(): void {
    this.authService.logout().then(() => {
      this.toastService.success('Logged out successfully');
      this.router.navigate(['/login']);
    });
  }
}

