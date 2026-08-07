import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';

export interface Department {
  id: number;
  departmentName: string;
  description: string;
}

export interface CreateDepartmentRequest {
  departmentName: string;
  description: string;
}

export interface AssignOfficerRequest {
  officerId: number;
}

@Injectable({
  providedIn: 'root'
})
export class DepartmentService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiUrl + '/departments';

  getAll(): Observable<Department[]> {
    return this.http.get<Department[]>(this.baseUrl);
  }

  getById(id: number): Observable<Department> {
    return this.http.get<Department>(`${this.baseUrl}/${id}`);
  }

  create(data: CreateDepartmentRequest): Observable<Department> {
    return this.http.post<Department>(this.baseUrl, data);
  }

  update(id: number, data: CreateDepartmentRequest): Observable<Department> {
    return this.http.put<Department>(`${this.baseUrl}/${id}`, data);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  assignOfficer(departmentId: number, officerId: number): Observable<any> {
    return this.http.post(`${this.baseUrl}/${departmentId}/assign-officer`, { officerId });
  }
}
