import { Component } from '@angular/core';
import { RouterOutlet, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { Topbar } from './components/topbar/topbar';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, Topbar],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  constructor(public router: Router) {}

  // Hide topbar on login and signup pages
  showTopbar(): boolean {
    const url = this.router.url;
    return !url.includes('/login') && !url.includes('/signup');
  }
}