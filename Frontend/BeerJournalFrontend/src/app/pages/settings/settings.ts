import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { AuthService } from '../../services/auth-service';// adjust path if needed

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './settings.html',
  styleUrl: './settings.css'
})
export class Settings implements OnInit {
  firstName = '';
  lastName = '';
  email = '';

  message = '';
  error = '';

  private apiUrl = '/api/Users';

  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) {}

  ngOnInit() {
    this.loadUser();
  }

  // Adds JWT token to protected API calls
  private getAuthHeaders() {
    const token = this.authService.getToken();

    return {
      headers: new HttpHeaders({
        Authorization: `Bearer ${token}`
      })
    };
  }

  // Loads logged-in user's profile
  loadUser() {
    this.http.get<any>(`${this.apiUrl}/me`, this.getAuthHeaders()).subscribe({
      next: (user) => {
        this.firstName = user.firstName;
        this.lastName = user.lastName;
        this.email = user.email;
      },
      error: () => {
        this.error = 'Could not load user details';
      }
    });
  }

  // Saves updated profile info
  updateUser() {
    const updatedUser = {
      firstName: this.firstName,
      lastName: this.lastName,
      email: this.email
    };

    this.http.put(`${this.apiUrl}/me`, updatedUser, this.getAuthHeaders()).subscribe({
      next: () => {
        this.message = 'Account updated successfully';
        this.error = '';
      },
      error: () => {
        this.error = 'Could not update account';
        this.message = '';
      }
    });
  }

  // Deletes the logged-in user's account
  deleteUser() {
    if (!confirm('Are you sure you want to delete your account?')) {
      return;
    }

    this.http.delete(`${this.apiUrl}/me`, this.getAuthHeaders()).subscribe({
      next: () => {
        this.authService.logout();
        window.location.href = '/login';
      },
      error: () => {
        this.error = 'Could not delete account';
      }
    });
  }
}