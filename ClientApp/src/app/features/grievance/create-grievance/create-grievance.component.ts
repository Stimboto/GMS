import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatSelectModule } from '@angular/material/select';
import { GrievanceService } from '../../../core/services/grievance.service';
import { DepartmentService } from '../../../core/services/department.service';
import { AiService, SimilarGrievance } from '../../../core/services/ai.service';
import { FileUploadComponent } from '../../../shared/components/file-upload/file-upload.component';
import { AiCardComponent } from '../../../shared/components/ai-card/ai-card.component';

@Component({
  selector: 'app-create-grievance',
  standalone: true,
  imports: [
    CommonModule, 
    ReactiveFormsModule, 
    RouterModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatSelectModule,
    FileUploadComponent,
    AiCardComponent
  ],
  templateUrl: './create-grievance.component.html',
  styleUrls: ['./create-grievance.component.css']
})
export class CreateGrievanceComponent implements OnInit {
  private fb = inject(FormBuilder);
  private grievanceService = inject(GrievanceService);
  private departmentService = inject(DepartmentService);
  private aiService = inject(AiService);
  private router = inject(Router);
  private snackBar = inject(MatSnackBar);

  grievanceForm: FormGroup = this.fb.group({
    title: ['', [Validators.required, Validators.maxLength(100)]],
    description: ['', [Validators.required, Validators.minLength(10)]],
    departmentId: [null, Validators.required]
  });

  loading = false;
  analyzing = false;
  selectedFiles: File[] = [];

  // AI Insights
  aiPriority?: string;
  aiSummary?: string;
  similarGrievances: SimilarGrievance[] = [];
  showSimilarAlert = false;

  departments: any[] = [];

  ngOnInit() {
    this.departmentService.getAll().subscribe({
      next: (deps) => {
        this.departments = deps.map(d => ({ id: d.id, name: d.departmentName }));
      },
      error: () => {
        this.snackBar.open('Failed to load departments', 'Close', { duration: 3000, panelClass: 'error-snackbar' });
      }
    });
  }

  onFilesChanged(files: File[]) {
    this.selectedFiles = files;
  }

  analyzeWithAI(title: string, description: string) {
    if (!title || !description) return;
    this.analyzing = true;
    this.showSimilarAlert = false;
    const req = { title, description };

    this.aiService.analyzeGrievance(req).subscribe({
      next: (res) => {
        this.analyzing = false;
        if (res.priority) this.aiPriority = res.priority;
        if (res.summary) this.aiSummary = res.summary;
        if (res.similarGrievances && res.similarGrievances.length > 0) {
          this.similarGrievances = res.similarGrievances;
          this.showSimilarAlert = true;
        }
      },
      error: () => {
        this.analyzing = false;
        this.snackBar.open('AI analysis completed with defaults.', 'Close', { duration: 3000 });
      }
    });
  }

  openGrievanceDetails(id: number) {
    window.open(`/citizen/grievances/${id}`, '_blank');
  }

  dismissSimilarAlert() {
    this.showSimilarAlert = false;
  }

  onSubmit() {
    if (this.grievanceForm.invalid) return;

    this.loading = true;
    
    const formData = new FormData();
    formData.append('title', this.grievanceForm.value.title);
    formData.append('description', this.grievanceForm.value.description);
    formData.append('departmentId', this.grievanceForm.value.departmentId);
    formData.append('category', 'General');
    formData.append('priority', this.aiPriority || 'Medium');
    if (this.aiSummary) formData.append('summary', this.aiSummary);
    
    if (this.selectedFiles && this.selectedFiles.length > 0) {
      formData.append('file', this.selectedFiles[0]);
    }

    this.grievanceService.create(formData).subscribe({
      next: () => {
        this.snackBar.open('Grievance submitted successfully!', 'Close', { duration: 3000, panelClass: 'success-snackbar' });
        this.router.navigate(['/citizen/grievances']);
      },
      error: () => {
        this.loading = false;
        this.snackBar.open('Failed to submit grievance. Please try again.', 'Close', { duration: 5000, panelClass: 'error-snackbar' });
      }
    });
  }
}
