// main.ts — the entry point of the Angular app
// This is the first file that runs when the browser loads our application

import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { App } from './app/app';

// bootstrapApplication starts the Angular app:
//   - 'App' is the root component (replaces <app-root> in index.html)
//   - 'appConfig' provides global setup (router, HttpClient, JWT interceptor)
// .catch logs any startup errors to the browser console
bootstrapApplication(App, appConfig)
  .catch((err) => console.error(err));