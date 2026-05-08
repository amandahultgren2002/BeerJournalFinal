// Auth service — talks to the backend for login/register and stores the JWT token

import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { tap } from 'rxjs';

@Injectable({
  providedIn: 'root'   // one shared instance for the whole app
})
export class AuthService {

  // All auth endpoints live under /api/auth (the proxy forwards to the backend)
  private baseUrl = '/api/auth';

  constructor(private http: HttpClient) {}

  // POST /api/auth/register — create a new account
  register(firstName: string, lastName: string, email: string, zipCode: number, password: string) {
    return this.http.post<any>(`${this.baseUrl}/register`, {
      firstName, lastName, email, zipCode, password
    });
  }

  // POST /api/auth/login — log in and save the token
  login(email: string, password: string) {
    return this.http.post<any>(`${this.baseUrl}/login`, { email, password })
      .pipe(
        // tap = run a side effect when the response arrives, without changing it
        tap(res => {
          // Save token + name in the browser so the user stays logged in
          localStorage.setItem('jwt_token', res.token);
          localStorage.setItem('first_name', res.firstName);
        })
      );
  }

  // Log out — just delete the saved values
  logout() {
    localStorage.removeItem('jwt_token');
    localStorage.removeItem('first_name');
  }

  // True if a token exists
  isLoggedIn(): boolean {
    return !!localStorage.getItem('jwt_token');
  }

  // Get the saved JWT token (used by the interceptor)
  getToken(): string | null {
    return localStorage.getItem('jwt_token');
  }

  // Get the user's first name (used in the topbar greeting)
  getFirstName(): string {
    return localStorage.getItem('first_name') ?? '';
  }
}