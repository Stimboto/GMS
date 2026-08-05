import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';
import { Grievance } from '../models/models';

@Injectable({
  providedIn: 'root'
})
export class GrievanceService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiUrl + '/grievances';

  create(data: any): Observable<Grievance> {
    return this.http.post<Grievance>(this.baseUrl, data);
  }

  getAll(): Observable<Grievance[]> {
    return this.http.get<Grievance[]>(this.baseUrl);
  }

  getMyGrievances(): Observable<Grievance[]> {
    return this.http.get<Grievance[]>(`${this.baseUrl}/my`);
  }

  getAssigned(): Observable<Grievance[]> {
    return this.http.get<Grievance[]>(`${this.baseUrl}/assigned`);
  }

  getById(id: number): Observable<Grievance> {
    return this.http.get<Grievance>(`${this.baseUrl}/${id}`);
  }

  update(id: number, data: any): Observable<any> {
    return this.http.put(`${this.baseUrl}/${id}`, data);
  }

  updateStatus(id: number, data: any): Observable<any> {
    return this.http.put(`${this.baseUrl}/${id}/status`, data);
  }

  assignOfficer(id: number, data: any): Observable<any> {
    return this.http.put(`${this.baseUrl}/${id}/assign`, data);
  }

  submitFeedback(id: number, data: { rating: number, remarks: string }): Observable<any> {
    return this.http.put(`${this.baseUrl}/${id}/feedback`, data);
  }
}
