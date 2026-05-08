// All routes in the app — the URL paths and which component to show

import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: 'signup', loadComponent: () => import('./pages/signup/signup').then(m => m.Signup) },
  { path: 'login', loadComponent: () => import('./pages/login/login').then(m => m.Login) },
  { path: 'journal', loadComponent: () => import('./pages/journal/journal').then(m => m.Journal) },

  // Create a new entry
  { path: 'log-beer', loadComponent: () => import('./pages/log-beer/log-beer').then(m => m.LogBeer) },

  // Edit an existing entry — :id is read from the URL
  { path: 'log-beer/:id', loadComponent: () => import('./pages/log-beer/log-beer').then(m => m.LogBeer) },

  { path: 'map', loadComponent: () => import('./pages/map/map').then(m => m.Map) },
  { path: 'settings', loadComponent: () => import('./pages/settings/settings').then(m => m.Settings) },

  // Default route — when the URL is empty, redirect to signup
  { path: '', redirectTo: 'signup', pathMatch: 'full' }
];