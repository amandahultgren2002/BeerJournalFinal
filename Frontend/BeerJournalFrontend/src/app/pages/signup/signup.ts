// Signup component — handles the create-account form

import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';        // for *ngIf
import { FormsModule, NgForm } from '@angular/forms';  // for ngModel and form validation
import { RouterLink, Router } from '@angular/router';
import { AuthService } from '../../services/auth-service';

@Component({
  selector: 'app-signup',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './signup.html',
  styleUrl: './signup.css'
})
export class Signup {
  // These match the form inputs in signup.html (two-way binding)
  firstName = '';
  lastName = '';
  email = '';
  zipCode: number | null = null;
  password = '';

  // UI state
  showPassword = false;     // toggles the password field visibility
  error: string | null = null;  // holds error message from the backend

  // Angular gives us these automatically (dependency injection)
  constructor(private auth: AuthService, private router: Router) {}

  // Runs when the user clicks "Create account"
  onSubmit(form: NgForm) {
    // Clear any old error message
    this.error = null;

    // If any field is invalid, show all error messages and stop
    if (form.invalid) {
      form.control.markAllAsTouched();   // makes all error messages appear
      return;
    }

    // All good — call the backend to register the user
    this.auth.register(this.firstName, this.lastName, this.email, this.zipCode!, this.password)
      .subscribe({
        next: () => this.router.navigate(['/login']),  // success → go to login
        error: (err) => this.error = err.error || 'Could not create account'
      });
  }

  // Switch between hidden and visible password
  togglePassword() {
    this.showPassword = !this.showPassword;
  }
}