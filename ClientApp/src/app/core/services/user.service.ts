import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface UserDto {
  id: number;
  fullName: string;
  email: string;
  phoneNumber?: string;
  role: string;
  isActive: boolean;
  createdAt: string;
  totalGrievances: number;
  resolvedGrievances: number;
  assignedCases: number;
  profileImageUrl?: string;
  emailNotificationsEnabled?: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/users`;

  getAllUsers(): Observable<UserDto[]> {
    return this.http.get<UserDto[]>(this.baseUrl);
  }

  getUserById(id: number): Observable<UserDto> {
    return this.http.get<UserDto>(`${this.baseUrl}/${id}`);
  }

  updateUserRole(id: number, role: string): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}/role`, { role });
  }

  updateUserStatus(id: number, isActive: boolean): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}/status`, { isActive });
  }

  deleteUser(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  // Profile Methods
  uploadProfileImage(file: File): Observable<{ profileImageUrl: string }> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.put<{ profileImageUrl: string }>(`${environment.apiUrl}/profile/image`, formData);
  }

  updatePreferences(emailNotificationsEnabled: boolean): Observable<void> {
    return this.http.put<void>(`${environment.apiUrl}/profile/preferences`, { emailNotificationsEnabled });
  }
}
