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

  constructor(private router: Router) {}

  logout() {
    localStorage.removeItem('jwt_token');
    localStorage.removeItem('first_name');
    this.router.navigate(['/login']);
  }
}