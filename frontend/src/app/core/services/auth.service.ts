import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, throwError } from 'rxjs';
import { PublicClientApplication, AuthenticationResult, Configuration, BrowserCacheLocation } from '@azure/msal-browser';
import { environment } from '@environments/environment';
import { CacheService } from './cache.service';

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
      cacheLocation: BrowserCacheLocation.LocalStorage,
      storeAuthStateInCookie: false
    }
  };

  private pca: PublicClientApplication;
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();
  private tokenKey = 'auth_token';
  private userKey = 'auth_user';
  private scopes = [`api://bb4ff7a1-4b89-4fb4-9e1c-9357ff252613/access_as_user`];
  private initPromise: Promise<void>;
  private loginInProgress = false;

  constructor(private http: HttpClient, private cacheService: CacheService) {
    this.pca = new PublicClientApplication(this.msalConfig);
    this.initPromise = this.initMSAL();
  }

  private async initMSAL(): Promise<void> {
    try {
      await this.pca.initialize();
      // Check if we're returning from a redirect
      const accounts = this.pca.getAllAccounts();
      if (accounts.length > 0) {
        const account = accounts[0];
        try {
          const response = await this.pca.acquireTokenSilent({
            scopes: this.scopes,
            account: account
          });
          this.setToken(response.accessToken);
          const user: User = {
            id: account.homeAccountId,
            name: account.name || 'User',
            email: account.username || ''
          };
          this.setUser(user);
        } catch (error) {
          console.log('Silent token acquisition failed (expected on first login):', error);
        }
      }
    } catch (error) {
      console.error('MSAL init error:', error);
    }
  }

  login(): Promise<void> {
    if (this.loginInProgress) {
      return Promise.reject(new Error('Login is already in progress'));
    }

    this.loginInProgress = true;
    
    return this.initPromise
      .catch(() => {
        // Reinitialize if initial init failed
        this.initPromise = this.initMSAL();
        return this.initPromise;
      })
      .then(() => {
        return new Promise<void>((resolve, reject) => {
          this.pca
            .loginPopup({
              scopes: this.scopes
            })
            .then((response: AuthenticationResult) => {
              this.setToken(response.accessToken);
              const user: User = {
                id: response.account?.homeAccountId || '',
                name: response.account?.name || 'User',
                email: response.account?.username || ''
              };
              this.setUser(user);
              this.loginInProgress = false;
              resolve();
            })
            .catch((error) => {
              this.loginInProgress = false;
              console.error('Login error:', error);
              reject(error);
            });
        });
      })
      .catch((error) => {
        this.loginInProgress = false;
        throw error;
      });
  }

  logout(): Promise<void> {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.userKey);
    this.cacheService.invalidate('current-user');
    this.currentUserSubject.next(null);

    return this.pca.logout({
      postLogoutRedirectUri: window.location.origin
    });
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

  restoreUserState(): void {
    const storedUser = this.getStoredUser();
    if (storedUser) {
      this.currentUserSubject.next(storedUser);
    }
  }

  getCurrentUser(): Observable<User> {
    const cacheKey = 'current-user';
    return this.cacheService.cacheRequest(cacheKey, this.http.get<User>(`${environment.apiUrl}/users/me`));
  }

  loginWithCredentials(email: string, password: string): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${environment.apiUrl}/auth/login`, { email, password });
  }

  refreshToken(): Observable<string> {
    const accounts = this.pca.getAllAccounts();
    if (accounts.length === 0) {
      return throwError(() => new Error('No accounts found'));
    }

    return new Observable((subscriber) => {
      this.pca.acquireTokenSilent({
        scopes: this.scopes,
        account: accounts[0]
      })
      .then((response) => {
        this.setToken(response.accessToken);
        subscriber.next(response.accessToken);
        subscriber.complete();
      })
      .catch((error) => {
        subscriber.error(error);
      });
    });
  }
}



