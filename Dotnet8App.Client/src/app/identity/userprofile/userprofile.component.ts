import {
  ChangeDetectionStrategy,
  Component,
  OnInit,
  signal,
} from '@angular/core';
import { faSignOutAlt } from '@fortawesome/free-solid-svg-icons';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { UserProfileDto } from '../service/identity.dto';
import { IdentityService } from '../service/identity.service';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-user-profile',
  standalone: true,
  imports: [FontAwesomeModule],
  templateUrl: './userprofile.component.html',
  styleUrl: './userprofile.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UserProfileComponent implements OnInit {
  icon = faSignOutAlt;

  profile = signal<UserProfileDto>({
    userName: null,
    userEMail: null,
  });

  constructor(private http: HttpClient, private identity: IdentityService) {}

  ngOnInit(): void {
    if (this.identity.jwtToken().accessToken) {
      this.identity.GetUserProfile().subscribe((res) => {
        this.profile.set(res);
      });
    }
  }

  SignOut(): void {
    this.identity.SignOut();
  }
}
