// Topbar — handles navigation bar and logout functionality

import { Component } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-topbar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  templateUrl: './topbar.html',
  styleUrl: './topbar.css'
})
export class Topbar {

  // Constructor — injects Angular Router so we can navigate programmatically
  constructor(private router: Router) {}

  // Logout — clears stored user data and redirects to login page
  logout() {
    // Remove authentication token and user name from local storage
    localStorage.removeItem('jwt_token');
    localStorage.removeItem('first_name');

    // Send the user back to the login page
    this.router.navigate(['/login']);
  }
}