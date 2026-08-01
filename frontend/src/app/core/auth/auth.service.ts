import { Injectable, computed, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthResult, LoginRequest, UserRole } from '../models/auth.models';

const TOKEN_KEY = 'library_auth_token';
const USER_KEY = 'library_auth_user';

@Injectable({ providedIn: 'root' })
export class AuthService {
  // Signal-based current user so components/guards can react without manual subscriptions.
  private readonly _currentUser = signal<AuthResult | null>(this.loadStoredUser());
  readonly currentUser = this._currentUser.asReadonly();
  readonly isAuthenticated = computed(() => !!this._currentUser());
  readonly role = computed(() => this._currentUser()?.role ?? null);

  constructor(private http: HttpClient, private router: Router) {}

  login(request: LoginRequest): Observable<AuthResult> {
    return this.http.post<AuthResult>(`${environment.apiUrl}/auth/login`, request).pipe(
      tap((result) => {
        localStorage.setItem(TOKEN_KEY, result.token);
        localStorage.setItem(USER_KEY, JSON.stringify(result));
        this._currentUser.set(result);
      })
    );
  }

  logout(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    this._currentUser.set(null);
    this.router.navigate(['/login']);
  }

  getToken(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }

  hasAnyRole(...roles: UserRole[]): boolean {
    const current = this.role();
    return current !== null && roles.includes(current);
  }

  private loadStoredUser(): AuthResult | null {
    const raw = localStorage.getItem(USER_KEY);
    if (!raw) return null;

    try {
      const user = JSON.parse(raw) as AuthResult;
      // Drop expired sessions on app load rather than trusting a stale token.
      if (new Date(user.expiresAt).getTime() <= Date.now()) {
        localStorage.removeItem(TOKEN_KEY);
        localStorage.removeItem(USER_KEY);
        return null;
      }
      return user;
    } catch {
      return null;
    }
  }
}
