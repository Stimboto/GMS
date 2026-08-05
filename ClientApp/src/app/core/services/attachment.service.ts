import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AttachmentService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiUrl + '/grievances';

  upload(grievanceId: number, file: File): Observable<any> {
    const formData = new FormData();
    formData.append('file', file);
    
    // Using reportProgress to optionally allow progress tracking
    return this.http.post(`${this.baseUrl}/${grievanceId}/attachments`, formData, {
      reportProgress: true,
      observe: 'events'
    });
  }
}
