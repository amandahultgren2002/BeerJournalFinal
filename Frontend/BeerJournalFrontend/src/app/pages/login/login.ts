// Login component — handles the login form

import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
import { RouterLink, Router } from '@angular/router';
import { AuthService } from '../../services/auth-service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class Login {
  // Form fields (bound to inputs in login.html)
  email = '';
  password = '';

  // UI state
  showPassword = false;
  error: string | null = null;

  constructor(private auth: AuthService, private router: Router) {}

  // Runs when the user clicks "Log in"
  onSubmit(form: NgForm) {
    // Reset old error so retries don't show stale messages
    this.error = null;

    // If the form is invalid, show all errors and stop
    if (form.invalid) {
      form.control.markAllAsTouched();
      return;
    }

    // Send credentials to the backend
    this.auth.login(this.email, this.password).subscribe({
      next: () => this.router.navigate(['/journal']),    // success → go to journal
      error: () => this.error = 'Invalid email or password'
    });
  }

  // Toggle password visibility
  togglePassword() {
    this.showPassword = !this.showPassword;
  }
}