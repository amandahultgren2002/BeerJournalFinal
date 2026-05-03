import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { tap } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

private baseUrl = '/api/auth';

  constructor(private http: HttpClient) {}

  register(firstName: string, lastName: string, email: string, password: string) {
    return this.http.post <any>(`${this.baseUrl}/register`, {
      firstName, lastName, email, password
    });
  }

  login(email: string, password: string) {
    return this.http.post <any>(`${this.baseUrl}/login`, { email, password })
      .pipe(
        tap(res => {
          localStorage.setItem('jwt_token', res.token);
          localStorage.setItem('first_name', res.firstName);
        })
      );
  }

  logout() {
    localStorage.removeItem('jwt_token');
    localStorage.removeItem('first_name');
  }

  isLoggedIn(): boolean {
    return !!localStorage.getItem('jwt_token');
  }

  getToken(): string | null {
    return localStorage.getItem('jwt_token');
  }

  getFirstName(): string {
    return localStorage.getItem('first_name') ?? '';
  }
}