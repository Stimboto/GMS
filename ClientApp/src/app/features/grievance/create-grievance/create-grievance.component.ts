import { Component, inject } from '@angular/core';
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
import { AiService } from '../../../core/services/ai.service';
import { FileUploadComponent } from '../../../shared/components/file-upload/file-upload.component';
import { AiCardComponent } from '../../../shared/components/ai-card/ai-card.component';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';

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
export class CreateGrievanceComponent {
  private fb = inject(FormBuilder);
  private grievanceService = inject(GrievanceService);
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
  aiCategory?: string;
  aiPriority?: string;

  departments = [
    { id: 1, name: 'Roads & Transport' },
    { id: 2, name: 'Water & Sanitation' },
    { id: 3, name: 'Electricity' },
    { id: 4, name: 'Public Health' },
    { id: 5, name: 'Other' }
  ];

  constructor() {
    this.grievanceForm.valueChanges
      .pipe(
        debounceTime(1500),
        distinctUntilChanged((prev, curr) => 
          prev.title === curr.title && prev.description === curr.description
        )
      )
      .subscribe(value => {
        if (value.title?.length > 5 && value.description?.length > 15) {
          this.analyzeWithAI(value.title, value.description);
        }
      });
  }

  onFilesChanged(files: File[]) {
    this.selectedFiles = files;
  }

  analyzeWithAI(title: string, description: string) {
    this.analyzing = true;
    const req = { title, description };

    forkJoin({
      category: this.aiService.predictCategory(req).pipe(catchError(() => of(null))),
      priority: this.aiService.predictPriority(req).pipe(catchError(() => of(null)))
    }).subscribe({
      next: (res) => {
        this.analyzing = false;
        if (res.category?.prediction) {
          this.aiCategory = res.category.prediction;
        }
        if (res.priority?.prediction) {
          this.aiPriority = res.priority.prediction;
        }
      },
      error: () => {
        this.analyzing = false;
      }
    });
  }

  onSubmit() {
    if (this.grievanceForm.invalid) return;

    this.loading = true;
    
    // In a real app with file uploads, this would be a FormData post.
    // For now, based on backend capabilities, we just send the JSON payload.
    // If backend file upload is supported for create, we'd adjust here.
    
    // We can merge AI insights if we want, or the backend does it automatically.
    // The previous implementation sent simple form values.
    
    const payload = {
      ...this.grievanceForm.value,
      category: this.aiCategory || 'Other',
      // Priority might be handled by backend AI completely, but if not we can pass it
    };

    this.grievanceService.create(payload).subscribe({
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
