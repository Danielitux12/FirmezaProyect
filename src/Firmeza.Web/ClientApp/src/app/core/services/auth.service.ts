import { Injectable, computed, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthResponseDto, LoginDto, RegisterDto, UserSession } from '../models/auth.model';

const TOKEN_KEY = 'firmeza_auth_token';
const USER_KEY = 'firmeza_auth_user';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly apiUrl = `${environment.apiUrl}/Auth`;

  private readonly _currentUser = signal<UserSession | null>(this.loadUserFromStorage());
  readonly currentUser = this._currentUser.asReadonly();

  readonly isAuthenticated = computed(() => !!this._currentUser());
  readonly isAdmin = computed(() => this._currentUser()?.role === 'Admin');
  readonly isClient = computed(() => this._currentUser()?.role === 'Client');
  readonly userRole = computed(() => this._currentUser()?.role ?? null);

  login(credentials: LoginDto): Observable<AuthResponseDto> {
    return this.http.post<AuthResponseDto>(`${this.apiUrl}/login`, credentials).pipe(
      tap((res) => {
        this.saveSession(res);
      })
    );
  }

  register(data: RegisterDto): Observable<AuthResponseDto> {
    return this.http.post<AuthResponseDto>(`${this.apiUrl}/register`, data).pipe(
      tap((res) => {
        this.saveSession(res);
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

  private saveSession(res: AuthResponseDto): void {
    localStorage.setItem(TOKEN_KEY, res.token);
    const session: UserSession = {
      email: res.email,
      role: res.role,
      clientId: res.clientId
    };
    localStorage.setItem(USER_KEY, JSON.stringify(session));
    this._currentUser.set(session);
  }

  private loadUserFromStorage(): UserSession | null {
    const raw = localStorage.getItem(USER_KEY);
    if (!raw) return null;
    try {
      return JSON.parse(raw) as UserSession;
    } catch {
      return null;
    }
  }
}
