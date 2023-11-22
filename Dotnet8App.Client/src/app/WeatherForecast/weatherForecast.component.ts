import { HttpClient, HttpClientModule } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';

interface WeatherForecast {
  date: string;
  temperatureC: number;
  temperatureF: number;
  summary: string;
}

@Component({
  selector: 'weatherForecast',
  standalone: true,
  imports: [HttpClientModule],
  templateUrl: './weatherForecast.component.html',
  styleUrl: './weatherForecast.component.scss',
})
export class WeatherForecastComponent implements OnInit {
  public forecasts: WeatherForecast[] = [];

  constructor(private http: HttpClient) {}

  ngOnInit() {
    this.getForecasts();
  }

  getForecasts() {
    this.http.get<WeatherForecast[]>('/weatherforecast').subscribe({
      next: (result) => {
        this.forecasts = result;
      },
      error: (error) => {
        console.error(error);
      },
    });
  }

  title = 'dotnet8app.client';
}
