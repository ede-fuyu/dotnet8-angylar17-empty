import { HttpClient } from '@angular/common/http';
import { Component, OnInit, signal } from '@angular/core';
import { IdentityService } from '../identity/service/identity.service';
import { first } from 'rxjs';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faRotate } from '@fortawesome/free-solid-svg-icons';

interface WeatherForecast {
  date: string;
  temperatureC: number;
  temperatureF: number;
  summary: string;
}

@Component({
  selector: 'weatherForecast',
  standalone: true,
  imports: [FontAwesomeModule],
  templateUrl: './weatherForecast.component.html',
  styleUrl: './weatherForecast.component.scss',
})
export class WeatherForecastComponent implements OnInit {
  icon = faRotate;

  forecasts = signal<WeatherForecast[]>([]);

  constructor(private http: HttpClient, private identity: IdentityService) {}

  ngOnInit() {
    this.identity.jwtToken$
      .pipe(first((res) => !!res.accessToken))
      .subscribe(() => this.getWeatherForecast());
  }

  getWeatherForecast() {
    this.http.get<WeatherForecast[]>('/weatherforecast').subscribe({
      next: (res) => this.forecasts.set(res),
      error: () => this.forecasts.set([]),
    });
  }
}
