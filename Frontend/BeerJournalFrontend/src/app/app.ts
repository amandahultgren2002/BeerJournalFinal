// Root app component — wraps the whole application
// Decides whether to show the topbar based on the current URL

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

  // Inject the Router so we can check the current URL
  // 'public' so the template can call router methods directly if needed
  constructor(public router: Router) {}

  // Returns true if the topbar should be visible
  // We hide it on login and signup pages so they look clean and focused
  showTopbar(): boolean {
    const url = this.router.url;
    return !url.includes('/login') && !url.includes('/signup');
  }
}