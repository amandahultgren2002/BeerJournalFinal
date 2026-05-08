// Settings component — lets the user view, update, or delete their account

import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { AuthService } from '../../services/auth-service';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './settings.html',
  styleUrl: './settings.css'
})
export class Settings implements OnInit {

  // Form fields — bound to the inputs in settings.html
  firstName = '';
  lastName = '';
  email = '';

  // Feedback messages shown under the Save button
  message = '';
  error = '';

  private apiUrl = '/api/Users';

  constructor(
    private http: HttpClient,
    private authService: AuthService,
    private cdr: ChangeDetectorRef       // tells Angular to refresh the view
  ) {}

  // Runs when the page loads — fetch the user's current details
  ngOnInit() {
    this.loadUser();
  }

  // Adds the JWT token to the request headers (so the backend allows it)
  private getAuthHeaders() {
    const token = this.authService.getToken();

    return {
      headers: new HttpHeaders({
        Authorization: `Bearer ${token}`
      })
    };
  }

  // Loads the logged-in user's profile from the backend
  loadUser() {
    this.http.get<any>(`${this.apiUrl}/me`, this.getAuthHeaders()).subscribe({
      next: (user) => {
        this.firstName = user.firstName;
        this.lastName = user.lastName;
        this.email = user.email;
        this.cdr.markForCheck();
      },
      error: () => {
        this.error = 'Could not load user details';
        this.cdr.markForCheck();
      }
    });
  }

  // Saves the updated profile info (PUT request)
  updateUser() {
    const updatedUser = {
      firstName: this.firstName,
      lastName: this.lastName,
      email: this.email
    };

    this.http.put(`${this.apiUrl}/me`, updatedUser, this.getAuthHeaders()).subscribe({
      next: () => {
        // Show success message and clear any old error
        this.message = 'Changes saved successfully';
        this.error = '';
        this.cdr.markForCheck();        // tell Angular to update the view immediately

        // Hide the message after 3 seconds so it doesn't stay forever
        setTimeout(() => {
          this.message = '';
          this.cdr.markForCheck();
        }, 3000);
      },
      error: () => {
        this.error = 'Could not update account';
        this.message = '';
        this.cdr.markForCheck();
      }
    });
  }

  // Deletes the user's account after a confirmation popup
  deleteUser() {
    if (!confirm('Are you sure you want to delete your account?')) {
      return;
    }

    this.http.delete(`${this.apiUrl}/me`, this.getAuthHeaders()).subscribe({
      next: () => {
        // Log the user out and send them to the login page
        this.authService.logout();
        window.location.href = '/login';
      },
      error: () => {
        this.error = 'Could not delete account';
        this.cdr.markForCheck();
      }
    });
  }
}