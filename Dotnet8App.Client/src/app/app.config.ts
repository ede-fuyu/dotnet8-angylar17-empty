import { ApplicationConfig, isDevMode } from '@angular/core';
import { provideRouter } from '@angular/router';
import { routes } from './app.routes';
import { provideStore } from '@ngrx/store';
import { provideEffects } from '@ngrx/effects';
import { provideStoreDevtools } from '@ngrx/store-devtools';
import { provideAnimations } from '@angular/platform-browser/animations';
import {
  HttpInterceptorFn,
  provideHttpClient,
  withInterceptors,
} from '@angular/common/http';
import { tap } from 'rxjs';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  let jwtToken: string | null = null;

  if (req.url.endsWith('refresh') || req.url.endsWith('signout')) {
    jwtToken = localStorage.getItem('RefreshToken');
  } else if (!req.url.includes('captcha') && !req.url.endsWith('signin')) {
    jwtToken = localStorage.getItem('AccessToken');
  }

  if (jwtToken) {
    const headers = req.headers.set('Authorization', `Bearer ${jwtToken}`);
    req = req.clone({ headers });
  }

  return next(req).pipe(tap((resp) => console.log('response', resp)));
};

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideHttpClient(withInterceptors([authInterceptor])),
    provideStore(),
    provideEffects(),
    provideStoreDevtools({ maxAge: 25, logOnly: !isDevMode() }),
    provideAnimations(),
  ],
};
