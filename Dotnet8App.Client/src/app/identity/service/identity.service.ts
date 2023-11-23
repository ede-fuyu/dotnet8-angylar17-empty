import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, computed, signal } from '@angular/core';
import { Observable, catchError, map, of, switchMap, take, tap } from 'rxjs';
import { LoginDto, LoginResponse, UserProfileDto } from './identity.dto';
import { toObservable } from '@angular/core/rxjs-interop';

@Injectable({
  providedIn: 'root',
})
export class IdentityService {
  private writableJwtToken = signal({
    accessToken: localStorage.getItem('AccessToken'),
    refreshToken: localStorage.getItem('RefreshToken'),
  });

  jwtToken = computed(() => this.writableJwtToken());

  jwtToken$ = toObservable(this.jwtToken);

  set accessToken(token: string | null) {
    if (token) {
      localStorage.setItem('AccessToken', token);
    } else {
      localStorage.removeItem('AccessToken');
    }

    this.writableJwtToken.update((jwt) => ({
      ...jwt,
      accessToken: token,
    }));
  }

  set refreshToken(token: string | null) {
    if (token) {
      localStorage.setItem('RefreshToken', token);
    } else {
      localStorage.removeItem('RefreshToken');
    }

    this.writableJwtToken.update((jwt) => ({
      ...jwt,
      refreshToken: token,
    }));
  }

  constructor(private http: HttpClient) {}

  GetCapcha(): Observable<string> {
    return this.http
      .get('/identity/captcha')
      .pipe(map((res) => `data:image/png;base64, ${res}`));
  }

  SignIn(login: LoginDto): Observable<LoginResponse> {
    return this.http.post<LoginResponse>('/identity/signin', login).pipe(
      tap((res) => {
        this.accessToken = res.accessToken;
        this.refreshToken = res.refreshToken;
      })
    );
  }

  RefreshJwtToken$(): Observable<boolean> {
    return this.jwtToken$.pipe(
      take(1),
      switchMap((res) => {
        if (res.accessToken && res.refreshToken) {
          const params = new HttpParams().set('token', res.accessToken);
          return this.http
            .get<LoginResponse>('/identity/refresh', { params })
            .pipe(
              tap((res) => {
                this.accessToken = res.accessToken;
                this.refreshToken = res.refreshToken;
              }),
              map(() => true),
              catchError(() => of(false)),
              tap((res) => {
                if (!res) {
                  this.accessToken = null;
                  this.refreshToken = null;
                }
              })
            );
        } else {
          return of(false);
        }
      })
    );
  }

  GetUserProfile() {
    return this.http.get<UserProfileDto>('/identity/GetUserProfile');
  }

  SignOut(): void {
    this.http.post('/identity/signout', {}).subscribe(() => {
      this.accessToken = null;
      this.refreshToken = null;
    });
  }
}
