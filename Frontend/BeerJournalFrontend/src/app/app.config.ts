// App configuration — startup setup for the whole frontend
// This is where we register all the global services Angular needs

import { ApplicationConfig, provideBrowserGlobalErrorListeners, provideZonelessChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { jwtInterceptor } from './interceptors/jwt-interceptor';
import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    // Logs uncaught browser errors so we see them in the console
    provideBrowserGlobalErrorListeners(),

    // New change detection style in Angular 18+ — faster, no Zone.js needed
    provideZonelessChangeDetection(),

    // Sets up the router with the routes from app.routes.ts
    provideRouter(routes),

    // Sets up HttpClient (used in all our services to call the backend)
    // withInterceptors() registers the JWT interceptor — it adds the JWT token
    // to every outgoing API call automatically, so we don't have to do it manually
    provideHttpClient(withInterceptors([jwtInterceptor]))
  ]
};