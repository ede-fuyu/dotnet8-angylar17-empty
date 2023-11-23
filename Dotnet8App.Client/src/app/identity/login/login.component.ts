import { HttpErrorResponse } from '@angular/common/http';
import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  signal,
} from '@angular/core';
import {
  FormBuilder,
  FormControl,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';

import { NgxSpinnerService } from 'ngx-spinner';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import {
  faEye,
  faEyeSlash,
  faKey,
  faSignInAlt,
  faUnlock,
  faUser,
  faUserShield,
} from '@fortawesome/free-solid-svg-icons';
import { MatSnackBar } from '@angular/material/snack-bar';
import { IdentityService } from '../service/identity.service';
import { LoginDto } from '../service/identity.dto';
import { from, switchMap } from 'rxjs';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, FontAwesomeModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginComponent implements OnInit {
  icon = signal({
    user: faUser,
    key: faKey,
    eye: faEye,
    shield: faUserShield,
    unlock: faUnlock,
    sign: faSignInAlt,
  });

  captcha = signal<string>('');

  formData = this.fb.group<{
    userName: FormControl<string | null>;
    password: FormControl<string | null>;
    captcha: FormControl<string | null>;
  }>({
    userName: new FormControl<string | null>(null, [Validators.required]),
    password: new FormControl<string | null>(null, [Validators.required]),
    captcha: new FormControl<string | null>(null, [Validators.required]),
  });

  constructor(
    private fb: FormBuilder,
    private identity: IdentityService,
    private spinner: NgxSpinnerService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.getCapcha();
  }

  getCapcha(): void {
    this.identity.GetCapcha().subscribe({
      next: (res) => this.captcha.set(res),
      complete: () => this.formData.controls.captcha.setValue(null),
    });
  }

  showPassword(passwd: HTMLInputElement): void {
    const recoverIconEye = () => {
      passwd.type = 'password';
      this.icon.update((icon) => ({
        ...icon,
        eye: faEye,
      }));
    };

    if (this.icon().eye === faEye) {
      passwd.type = 'text';
      this.icon.update((icon) => ({
        ...icon,
        eye: faEyeSlash,
      }));

      setTimeout(() => recoverIconEye(), 5000);
    } else {
      recoverIconEye();
    }
  }

  SignIn(): void {
    if (this.formData.valid) {
      const login = this.formData.getRawValue() as LoginDto;
      from(this.spinner.show())
        .pipe(switchMap(() => this.identity.SignIn(login)))
        .subscribe({
          error: (err: HttpErrorResponse) => {
            this.getCapcha();
            if (err.status === 401) {
              this.snackBar.open(err.error.detail, 'x', {
                duration: 5000,
                panelClass: ['error'],
                horizontalPosition: 'left',
                verticalPosition: 'top',
              });
            } else {
              this.snackBar.open('帳號或密碼輸入錯誤!!', 'x', {
                duration: 5000,
                panelClass: ['error'],
                horizontalPosition: 'left',
                verticalPosition: 'top',
              });
            }
            this.spinner.hide();
          },
          complete: () => this.spinner.hide(),
        });
    }
  }
}
