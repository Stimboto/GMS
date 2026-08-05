import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';
import { DashboardStats } from '../models/models';

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiUrl + '/dashboard';

  getCitizenDashboard(): Observable<DashboardStats> {
    return this.http.get<DashboardStats>(`${this.baseUrl}/citizen`);
  }

  getOfficerDashboard(): Observable<DashboardStats> {
    return this.http.get<DashboardStats>(`${this.baseUrl}/officer`);
  }

  getAdminDashboard(): Observable<DashboardStats> {
    return this.http.get<DashboardStats>(`${this.baseUrl}/admin`);
  }

  getStatusChart(): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/charts/status`);
  }

  getDepartmentChart(): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/charts/departments`);
  }

  getMonthlyChart(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/charts/monthly`);
  }
}
