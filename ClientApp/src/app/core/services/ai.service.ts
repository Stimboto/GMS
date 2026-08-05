import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';

export interface AiPredictionRequest {
  title: string;
  description: string;
}

export interface AiPredictionResponse {
  prediction: string;
}

@Injectable({
  providedIn: 'root'
})
export class AiService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiUrl + '/ai';

  predictCategory(data: AiPredictionRequest): Observable<AiPredictionResponse> {
    return this.http.post<AiPredictionResponse>(`${this.baseUrl}/predict-category`, data);
  }

  predictPriority(data: AiPredictionRequest): Observable<AiPredictionResponse> {
    return this.http.post<AiPredictionResponse>(`${this.baseUrl}/predict-priority`, data);
  }

  generateSummary(data: AiPredictionRequest): Observable<AiPredictionResponse> {
    return this.http.post<AiPredictionResponse>(`${this.baseUrl}/generate-summary`, data);
  }
}
