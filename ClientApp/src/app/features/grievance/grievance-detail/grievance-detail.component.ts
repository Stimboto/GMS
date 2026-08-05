import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatDividerModule } from '@angular/material/divider';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar } from '@angular/material/snack-bar';
import { GrievanceService } from '../../../core/services/grievance.service';
import { AttachmentService } from '../../../core/services/attachment.service';
import { Grievance, User } from '../../../core/models/models';
import { AuthService } from '../../../core/services/auth.service';
import { UserService, UserDto } from '../../../core/services/user.service';
import { VerticalTimelineComponent } from '../../../shared/components/vertical-timeline/vertical-timeline.component';
import { AiCardComponent } from '../../../shared/components/ai-card/ai-card.component';
import { FileUploadComponent } from '../../../shared/components/file-upload/file-upload.component';
import { SkeletonComponent } from '../../../shared/components/skeleton/skeleton.component';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-grievance-detail',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatDividerModule,
    MatChipsModule,
    MatIconModule,
    MatButtonModule,
    MatProgressBarModule,
    MatProgressSpinnerModule,
    MatSelectModule,
    VerticalTimelineComponent,
    AiCardComponent,
    FileUploadComponent,
    SkeletonComponent,
    FormsModule
  ],
  templateUrl: './grievance-detail.component.html',
  styleUrls: ['./grievance-detail.component.css']
})
export class GrievanceDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private grievanceService = inject(GrievanceService);
  private attachmentService = inject(AttachmentService);
  private snackBar = inject(MatSnackBar);
  private authService = inject(AuthService);
  private userService = inject(UserService);
  
  grievance: Grievance | null = null;
  loading = true;
  user: User | null = null;
  
  // Attachments
  selectedFiles: File[] = [];
  uploading = false;

  // Actions
  actionLoading = false;
  feedbackRating = 0;
  feedbackRemarks = '';
  feedbackLoading = false;

  // Admin Assign
  officers: UserDto[] = [];
  selectedOfficerId: number | null = null;

  ngOnInit() {
    this.user = this.authService.currentUser;
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadGrievance(+id);
    }
  }

  loadGrievance(id: number) {
    this.loading = true;
    this.grievanceService.getById(id).subscribe({
      next: (res) => {
        this.grievance = res;
        this.loading = false;
        
        if (this.user?.role === 'Admin') {
          this.loadOfficers();
        }
      },
      error: () => this.loading = false
    });
  }

  loadOfficers() {
    this.userService.getAllUsers().subscribe({
      next: (users) => {
        this.officers = users.filter(u => u.role === 'Officer');
      }
    });
  }

  onFilesChanged(files: File[]) {
    this.selectedFiles = files;
  }

  onUpload() {
    if (this.selectedFiles.length > 0 && this.grievance) {
      this.uploading = true;
      
      // Upload first file for simplicity in this demo.
      // A real app would loop or use a multi-upload endpoint.
      this.attachmentService.upload(this.grievance.id, this.selectedFiles[0]).subscribe({
        next: () => {
          this.uploading = false;
          this.selectedFiles = [];
          this.loadGrievance(this.grievance!.id);
          this.snackBar.open('File uploaded successfully', 'Close', { duration: 3000, panelClass: 'success-snackbar' });
        },
        error: () => {
          this.uploading = false;
          this.snackBar.open('Upload failed', 'Close', { duration: 5000, panelClass: 'error-snackbar' });
        }
      });
    }
  }

  updateStatus(newStatus: string) {
    if (!this.grievance) return;
    this.actionLoading = true;
    this.grievanceService.updateStatus(this.grievance.id, { status: newStatus, remarks: 'Updated by ' + this.user?.role }).subscribe({
      next: () => {
        this.actionLoading = false;
        this.snackBar.open(`Status updated to ${newStatus}`, 'Close', { duration: 3000, panelClass: 'success-snackbar' });
        this.loadGrievance(this.grievance!.id);
      },
      error: () => {
        this.actionLoading = false;
        this.snackBar.open('Failed to update status', 'Close', { duration: 5000, panelClass: 'error-snackbar' });
      }
    });
  }

  setRating(rating: number) {
    this.feedbackRating = rating;
  }

  submitFeedback() {
    if (!this.grievance || this.feedbackRating === 0) return;
    this.feedbackLoading = true;
    this.grievanceService.submitFeedback(this.grievance.id, {
      rating: this.feedbackRating,
      remarks: this.feedbackRemarks
    }).subscribe({
      next: () => {
        this.feedbackLoading = false;
        this.snackBar.open('Feedback submitted successfully', 'Close', { duration: 3000, panelClass: 'success-snackbar' });
        this.loadGrievance(this.grievance!.id);
      },
      error: () => {
        this.feedbackLoading = false;
        this.snackBar.open('Failed to submit feedback', 'Close', { duration: 5000, panelClass: 'error-snackbar' });
      }
    });
  }

  assignOfficer() {
    if (!this.grievance || !this.selectedOfficerId) return;
    this.actionLoading = true;
    this.grievanceService.assignOfficer(this.grievance.id, { officerId: this.selectedOfficerId }).subscribe({
      next: () => {
        this.actionLoading = false;
        this.snackBar.open('Officer assigned successfully', 'Close', { duration: 3000, panelClass: 'success-snackbar' });
        this.loadGrievance(this.grievance!.id);
      },
      error: () => {
        this.actionLoading = false;
        this.snackBar.open('Failed to assign officer', 'Close', { duration: 5000, panelClass: 'error-snackbar' });
      }
    });
  }

  getPriorityColor(priority: string): string {
    switch(priority?.toLowerCase()) {
      case 'high': return 'priority-high';
      case 'medium': return 'priority-medium';
      case 'low': return 'priority-low';
      default: return '';
    }
  }

  getStatusColor(status: string): string {
    switch(status?.toLowerCase()) {
      case 'pending': return 'status-pending';
      case 'in progress':
      case 'in review': return 'status-review';
      case 'resolved': return 'status-resolved';
      default: return '';
    }
  }

  getFileIcon(filename: string): string {
    const ext = filename.split('.').pop()?.toLowerCase();
    switch(ext) {
      case 'pdf': return 'picture_as_pdf';
      case 'jpg':
      case 'jpeg':
      case 'png': return 'image';
      case 'doc':
      case 'docx': return 'description';
      default: return 'insert_drive_file';
    }
  }
}
