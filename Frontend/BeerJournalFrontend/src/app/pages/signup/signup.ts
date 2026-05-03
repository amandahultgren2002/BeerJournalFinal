import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
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
  firstName = '';
  lastName = '';
  email = '';
  password = '';
  showPassword = false;
  error: string | null = null;

  constructor(private auth: AuthService, private router: Router) {}

  onSubmit() {
    if (!this.firstName || !this.lastName || !this.email || !this.password) return;

    this.auth.register(this.firstName, this.lastName, this.email, this.password).subscribe({
      next: () => this.router.navigate(['/login']),
      error: (err) => this.error = err.error || 'Could not create account'
    });
  }

  togglePassword() {
    this.showPassword = !this.showPassword;
  }
}