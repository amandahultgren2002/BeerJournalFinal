import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: 'signup', loadComponent: () => import('./pages/signup/signup').then(m => m.Signup) },
{ path: 'login', loadComponent: () => import('./pages/login/login').then(m => m.Login) },
  { path: 'journal', loadComponent: () => import('./pages/journal/journal').then(m => m.Journal) },
    { path: 'log-beer', loadComponent: () => import('./pages/log-beer/log-beer').then(m => m.LogBeer) },
      { path: 'map', loadComponent: () => import('./pages/map/map').then(m => m.Map) },
        { path: 'settings', loadComponent: () => import('./pages/settings/settings').then(m => m.Settings) },
  { path: '', redirectTo: 'signup', pathMatch: 'full' }
];