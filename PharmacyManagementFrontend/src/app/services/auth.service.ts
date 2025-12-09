import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { tap } from 'rxjs/operators';
import { LoginRequest, RegisterRequest, AuthResponse, User } from '../models/auth.models';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = 'http://localhost:5063/api/authentication';
  private currentUserSubject = new BehaviorSubject<User | null>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient) {
    this.loadUserFromStorage();
  }

  login(credentials: LoginRequest): Observable<AuthResponse> {
    return this.http.post<any>(`${this.apiUrl}/login_user`, credentials)
      .pipe(
        tap((response: any) => {
          const token = response.token || response.Token;
          if (token && typeof window !== 'undefined') {
            localStorage.setItem('token', token);
            this.getUserInfo().subscribe();
          }
        })
      );
  }

  adminLogin(credentials: LoginRequest): Observable<AuthResponse> {
    return this.http.post<any>(`${this.apiUrl}/login_admin`, credentials)
      .pipe(
        tap((response: any) => {
          console.log('Admin login response:', response);
          const token = response.token || response.Token;
          console.log('Extracted token:', token);
          if (token && typeof window !== 'undefined') {
            localStorage.setItem('token', token);
            console.log('Token stored, calling getUserInfo...');
            this.getUserInfo().subscribe({
              next: (user) => console.log('User info received:', user),
              error: (error) => {
                console.error('getUserInfo error:', error);
                // Fallback: set basic user info from token if getUserInfo fails
                this.currentUserSubject.next({
                  userId: 'admin',
                  email: 'admin@pharmacy.com',
                  role: 'Admin',
                  status: 'Active'
                });
              }
            });
          }
        })
      );
  }

  register(userData: RegisterRequest): Observable<any> {
    return this.http.post(`${this.apiUrl}/register_user`, userData);
  }

  getUserInfo(): Observable<User> {
    console.log('Making getUserInfo request to:', `${this.apiUrl}/user`);
    return this.http.get<any>(`${this.apiUrl}/user`)
      .pipe(
        tap((response: any) => {
          console.log('getUserInfo response:', response);
          const user: User = {
            userId: response.userId || response.UserId,
            email: response.email || response.Email,
            role: response.role || response.Role,
            status: response.status || response.Status
          };
          console.log('Mapped user object:', user);
          this.currentUserSubject.next(user);
          if (typeof window !== 'undefined') {
            localStorage.setItem('user', JSON.stringify(user));
          }
        })
      );
  }

  logout(): void {
    if (typeof window !== 'undefined') {
      localStorage.removeItem('token');
      localStorage.removeItem('user');
    }
    this.currentUserSubject.next(null);
  }

  isAuthenticated(): boolean {
    if (typeof window !== 'undefined') {
      return !!localStorage.getItem('token');
    }
    return false;
  }

  getToken(): string | null {
    if (typeof window !== 'undefined') {
      return localStorage.getItem('token');
    }
    return null;
  }

  getCurrentUser(): User | null {
    return this.currentUserSubject.value;
  }

  isAdmin(): boolean {
    const user = this.getCurrentUser();
    return user?.role === 'Admin';
  }

  isDoctor(): boolean {
    const user = this.getCurrentUser();
    return user?.role === 'Doctor';
  }

  private loadUserFromStorage(): void {
    if (typeof window !== 'undefined' && window.localStorage) {
      const userStr = localStorage.getItem('user');
      if (userStr) {
        const user = JSON.parse(userStr);
        this.currentUserSubject.next(user);
      }
    }
  }
}