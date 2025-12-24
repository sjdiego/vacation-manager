import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { PublicClientApplication, AuthenticationResult, Configuration } from '@azure/msal-browser';
import { environment } from '@environments/environment';

export interface User {
  id: string;
  name: string;
  email: string;
  department?: string;
}

export interface LoginResponse {
  token: string;
  user: User;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private msalConfig: Configuration = {
    auth: {
      clientId: environment.auth.clientId,
      authority: environment.auth.authority,
      redirectUri: environment.auth.redirectUri,
      postLogoutRedirectUri: environment.auth.redirectUri
    },
    cache: {
      cacheLocation: 'localStorage',
      storeAuthStateInCookie: false
    }
  };

  private pca: PublicClientApplication;
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();
  private tokenKey = 'auth_token';
  private userKey = 'auth_user';
  private scopes = ['User.Read'];

  constructor(private http: HttpClient) {
    this.pca = new PublicClientApplication(this.msalConfig);
    this.initializeMSAL();
  }

  private async initializeMSAL(): Promise<void> {
    try {
      await this.pca.initialize();
      // Check if returning from redirect
      const response = this.pca.getAllAccounts();
      if (response.length > 0) {
        await this.handleLoginSuccess();
      }
    } catch (error) {
      console.error('MSAL initialization failed:', error);
    }
  }

  login(): Promise<void> {
    return new Promise((resolve, reject) => {
      this.pca
        .loginPopup({
          scopes: this.scopes
        })
        .then((response: AuthenticationResult) => {
          this.handleLoginSuccess().then(() => resolve()).catch(() => reject());
        })
        .catch((error) => {
          console.error('MSAL login error:', error);
          reject(error);
        });
    });
  }

  private async handleLoginSuccess(): Promise<void> {
    const accounts = this.pca.getAllAccounts();
    if (accounts.length > 0) {
      const account = accounts[0];

      try {
        const response = await this.pca.acquireTokenSilent({
          scopes: this.scopes,
          account: account
        });

        // Store token
        this.setToken(response.accessToken);

        // Extract user info from the token
        const user: User = {
          id: account.homeAccountId,
          name: account.name || 'User',
          email: account.username || ''
        };

        this.setUser(user);

        // Try to register user in backend
        this.registerUserInBackend(user).subscribe({
          next: () => {
            console.log('User registered in backend');
          },
          error: (error) => {
            console.warn('Could not register user in backend:', error);
            // Still allow login even if backend registration fails
          }
        });
      } catch (error) {
        console.error('Token acquisition failed:', error);
        throw error;
      }
    }
  }

  private registerUserInBackend(user: User): Observable<any> {
    return this.http.post(`${environment.apiUrl}/users/register`, {});
  }

  logout(): Promise<void> {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.userKey);
    this.currentUserSubject.next(null);

    return this.pca.logout({
      postLogoutRedirectUri: environment.auth.redirectUri
    });
  }

  loginWithCredentials(email: string, password: string): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${environment.apiUrl}/auth/login`, { email, password });
  }

  getCurrentUser(): Observable<User> {
    return this.http.get<User>(`${environment.apiUrl}/auth/me`);
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  setToken(token: string): void {
    localStorage.setItem(this.tokenKey, token);
  }

  setUser(user: User): void {
    localStorage.setItem(this.userKey, JSON.stringify(user));
    this.currentUserSubject.next(user);
  }

  getStoredUser(): User | null {
    const user = localStorage.getItem(this.userKey);
    return user ? JSON.parse(user) : null;
  }

  getCurrentAccount() {
    return this.pca.getAllAccounts()[0] || null;
  }
}
