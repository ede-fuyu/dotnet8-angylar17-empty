import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { LoginComponent } from './identity/login/login.component';
import { IdentityService } from './identity/service/identity.service';
import { UserProfileComponent } from './identity/userprofile/userprofile.component';
import { HttpClient } from '@angular/common/http';
import { NgxSpinnerModule } from 'ngx-spinner';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule,
    RouterOutlet,
    NgxSpinnerModule,
    LoginComponent,
    UserProfileComponent,
  ],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss',
})
export class AppComponent implements OnInit {
  chackLogin = signal<boolean>(true);

  title = 'dotnet8 and angular 17+';

  constructor(private http: HttpClient, private identity: IdentityService) {}

  ngOnInit(): void {
    this.identity
      .RefreshJwtToken$()
      .subscribe((res) => this.chackLogin.set(res));

    this.identity.jwtToken$.subscribe((res) =>
      this.chackLogin.set(!!res.accessToken)
    );
  }
}
