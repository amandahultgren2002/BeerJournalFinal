import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
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
  email = '';
  password = '';
  showPassword = false;
  error: string | null = null;

constructor(private auth: AuthService, private router: Router) {}  
onSubmit() {
    if (!this.email || !this.password) return;

    this.auth.login(this.email, this.password).subscribe({
      next: () => this.router.navigate(['/journal']),
      error: () => this.error = 'Invalid email or password'
    });
  }

  togglePassword() {
    this.showPassword = !this.showPassword;
  }
}