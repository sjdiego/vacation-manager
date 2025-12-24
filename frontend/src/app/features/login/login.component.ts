import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '@app/core/services/auth.service';
import { ToastService } from '@app/core/services/toast.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {
  isLoading = false;

  constructor(
    private authService: AuthService,
    private toastService: ToastService,
    private router: Router
  ) {}

  ngOnInit(): void {
    // If already authenticated, redirect to home
    if (this.authService.isAuthenticated()) {
      this.router.navigate(['/vacations']);
    }
  }

  loginWithMSAL(): void {
    if (this.isLoading) return; // Prevent multiple clicks
    
    this.isLoading = true;
    this.authService.login().then(
      () => {
        this.isLoading = false;
        this.toastService.success('Login successful!');
        this.router.navigate(['/vacations']);
      },
      (error) => {
        this.isLoading = false;
        console.error('Login error:', error);
        this.toastService.error('Login failed. Please try again.');
      }
    );
  }
}
