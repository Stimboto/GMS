import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';

export interface AiPredictionRequest {
  title: string;
  description: string;
}

export interface SimilarGrievance {
  id: number;
  trackingId: string;
  title: string;
  status: string;
  department: string;
  createdAt: string;
}

export interface GrievanceAnalysisResult {
  priority: string;
  summary: string;
  similarGrievances: SimilarGrievance[];
}

export interface ChatMessage {
  sender: 'user' | 'bot';
  text: string;
}

export interface ChatRequest {
  message: string;
  history: ChatMessage[];
}

export interface ChatResponse {
  reply: string;
}

@Injectable({
  providedIn: 'root'
})
export class AiService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiUrl + '/ai';

  analyzeGrievance(data: AiPredictionRequest): Observable<GrievanceAnalysisResult> {
    return this.http.post<GrievanceAnalysisResult>(`${this.baseUrl}/analyze`, data);
  }

  chat(data: ChatRequest): Observable<ChatResponse> {
    return this.http.post<ChatResponse>(`${this.baseUrl}/chat`, data);
  }
}
