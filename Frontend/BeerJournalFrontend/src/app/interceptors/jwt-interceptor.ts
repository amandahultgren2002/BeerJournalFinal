// JWT Interceptor — attaches JWT token to outgoing HTTP requests

import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth-service';

export const jwtInterceptor: HttpInterceptorFn = (req, next) => {
  // Get token from local storage
  const token = localStorage.getItem('jwt_token');

  // If token exists, add it to Authorization header
  if (token) {
    req = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }

  // Pass request to next handler (API call continues)
  return next(req);
};