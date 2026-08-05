import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { AuthResponse, User } from '../models/models';
import { BehaviorSubject, tap, Observable } from 'rxjs';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);
  private baseUrl = environment.apiUrl + '/auth';

  private currentUserSubject = new BehaviorSubject<User | null>(this.parseToken());
  public currentUser$ = this.currentUserSubject.asObservable();

  login(data: any): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.baseUrl}/login`, data).pipe(
      tap(res => {
        localStorage.setItem('token', res.token);
        this.saveExtraUserData(res);
        this.currentUserSubject.next(this.parseToken());
      })
    );
  }

  register(data: any): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.baseUrl}/register`, data).pipe(
      tap(res => {
        localStorage.setItem('token', res.token);
        this.saveExtraUserData(res);
        this.currentUserSubject.next(this.parseToken());
      })
    );
  }

  logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('userData');
    this.currentUserSubject.next(null);
    this.router.navigate(['/login']);
  }

  getToken(): string | null {
    return localStorage.getItem('token');
  }

  updateUser(user: User) {
    // Save to local storage for persistence across reloads
    const extraData = {
      profileImageUrl: user.profileImageUrl,
      emailNotificationsEnabled: user.emailNotificationsEnabled
    };
    localStorage.setItem('userData', JSON.stringify(extraData));
    
    // Update the observable
    this.currentUserSubject.next(user);
  }

  private saveExtraUserData(res: any) {
    if (res.profileImageUrl !== undefined || res.emailNotificationsEnabled !== undefined) {
      const extraData = {
        profileImageUrl: res.profileImageUrl,
        emailNotificationsEnabled: res.emailNotificationsEnabled
      };
      localStorage.setItem('userData', JSON.stringify(extraData));
    }
  }

  private parseToken(): User | null {
    const token = localStorage.getItem('token');
    if (!token) return null;
    
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      let extraData: any = {};
      const extraStr = localStorage.getItem('userData');
      if (extraStr) {
        try {
          extraData = JSON.parse(extraStr);
        } catch (e) {}
      }

      return {
        id: parseInt(payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] || payload['nameid'] || payload['sub'] || '0'),
        fullName: payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] || payload['unique_name'] || '',
        email: payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] || payload['email'] || '',
        role: payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || payload['role'] || '',
        profileImageUrl: extraData.profileImageUrl || payload['profileImageUrl'],
        emailNotificationsEnabled: extraData.emailNotificationsEnabled !== undefined ? extraData.emailNotificationsEnabled : payload['emailNotificationsEnabled']
      };
    } catch (e) {
      return null;
    }
  }

  get currentUser(): User | null {
    return this.currentUserSubject.value;
  }
}
