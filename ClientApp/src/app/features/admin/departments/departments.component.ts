import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialogModule, MatDialog, MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { DepartmentService, Department } from '../../../core/services/department.service';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { UserService, UserDto } from '../../../core/services/user.service';
// Reusable inline dialog component for creating/editing department
@Component({
  selector: 'app-department-dialog',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatFormFieldModule, MatInputModule, MatButtonModule, MatDialogModule],
  template: `
    <h2 mat-dialog-title>{{ data?.id ? 'Edit' : 'Create' }} Department</h2>
    <mat-dialog-content>
      <form [formGroup]="form" class="flex flex-col gap-4 mt-4">
        <mat-form-field appearance="outline">
          <mat-label>Department Name</mat-label>
          <input matInput formControlName="departmentName" placeholder="e.g. Public Works">
          <mat-error *ngIf="form.get('departmentName')?.hasError('required')">Name is required</mat-error>
        </mat-form-field>
        <mat-form-field appearance="outline">
          <mat-label>Description</mat-label>
          <textarea matInput formControlName="description" rows="3"></textarea>
        </mat-form-field>
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button mat-dialog-close>Cancel</button>
      <button mat-raised-button color="primary" [disabled]="form.invalid" (click)="save()">Save</button>
    </mat-dialog-actions>
  `
})
export class DepartmentDialogComponent {
  form: FormGroup;
  data = inject(MAT_DIALOG_DATA);
  dialogRef = inject(MatDialogRef<DepartmentDialogComponent>);
  
  constructor(private fb: FormBuilder) {
    this.form = this.fb.group({
      departmentName: [this.data?.departmentName || '', Validators.required],
      description: [this.data?.description || '']
    });
  }

  save() {
    if (this.form.valid) {
      this.dialogRef.close(this.form.value);
    }
  }
}

@Component({
  selector: 'app-assign-officer-dialog',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatFormFieldModule, MatSelectModule, MatButtonModule, MatDialogModule],
  template: `
    <h2 mat-dialog-title>Assign Officer to {{ data?.departmentName }}</h2>
    <mat-dialog-content>
      <form [formGroup]="form" class="flex flex-col gap-4 mt-4">
        <mat-form-field appearance="outline">
          <mat-label>Select Officer</mat-label>
          <mat-select formControlName="officerId">
            <mat-option *ngFor="let officer of officers" [value]="officer.id">
              {{ officer.fullName }}
            </mat-option>
          </mat-select>
          <mat-error *ngIf="form.get('officerId')?.hasError('required')">Officer is required</mat-error>
        </mat-form-field>
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button mat-dialog-close>Cancel</button>
      <button mat-raised-button color="primary" [disabled]="form.invalid" (click)="save()">Assign</button>
    </mat-dialog-actions>
  `
})
export class AssignOfficerDialogComponent implements OnInit {
  form: FormGroup;
  data = inject(MAT_DIALOG_DATA);
  dialogRef = inject(MatDialogRef<AssignOfficerDialogComponent>);
  userService = inject(UserService);
  officers: UserDto[] = [];
  
  constructor(private fb: FormBuilder) {
    this.form = this.fb.group({
      officerId: ['', Validators.required]
    });
  }

  ngOnInit() {
    this.userService.getAllUsers().subscribe((users: UserDto[]) => {
      this.officers = users.filter((u: UserDto) => u.role === 'Officer');
    });
  }

  save() {
    if (this.form.valid) {
      this.dialogRef.close(this.form.value);
    }
  }
}

@Component({
  selector: 'app-departments',
  standalone: true,
  imports: [CommonModule, MatTableModule, MatButtonModule, MatIconModule, MatDialogModule, MatProgressSpinnerModule],
  template: `
    <div class="p-4 md:p-8 w-full min-h-full">
      <div class="flex justify-between items-center mb-6">
        <div>
          <h1 class="text-3xl font-bold text-gray-800">Department Management</h1>
          <p class="text-gray-600">Configure system departments and assign officers.</p>
        </div>
        <button mat-raised-button color="primary" (click)="openDialog()">
          <mat-icon>add</mat-icon> Add Department
        </button>
      </div>

      <div *ngIf="loading" class="flex justify-center my-12">
        <mat-spinner diameter="40"></mat-spinner>
      </div>

      <div *ngIf="!loading && departments.length === 0" class="text-center text-gray-500 my-12">
        <mat-icon class="text-5xl mb-4">domain_disabled</mat-icon>
        <p>No departments found. Create one to get started.</p>
      </div>

      <table mat-table [dataSource]="departments" class="mat-elevation-z2 w-full" *ngIf="!loading && departments.length > 0">
        <ng-container matColumnDef="id">
          <th mat-header-cell *matHeaderCellDef> ID </th>
          <td mat-cell *matCellDef="let element"> {{element.id}} </td>
        </ng-container>

        <ng-container matColumnDef="name">
          <th mat-header-cell *matHeaderCellDef> Name </th>
          <td mat-cell *matCellDef="let element"> {{element.departmentName}} </td>
        </ng-container>

        <ng-container matColumnDef="description">
          <th mat-header-cell *matHeaderCellDef> Description </th>
          <td mat-cell *matCellDef="let element"> {{element.description}} </td>
        </ng-container>

        <ng-container matColumnDef="actions">
          <th mat-header-cell *matHeaderCellDef> Actions </th>
          <td mat-cell *matCellDef="let element">
            <button mat-icon-button color="primary" (click)="openDialog(element)" title="Edit Department">
              <mat-icon>edit</mat-icon>
            </button>
            <button mat-icon-button color="accent" (click)="assignOfficer(element)" title="Assign Officer">
              <mat-icon>person_add</mat-icon>
            </button>
            <button mat-icon-button color="warn" (click)="deleteDepartment(element.id)" title="Delete Department">
              <mat-icon>delete</mat-icon>
            </button>
          </td>
        </ng-container>

        <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
        <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
      </table>
    </div>
  `
})
export class DepartmentsComponent implements OnInit {
  private departmentService = inject(DepartmentService);
  private dialog = inject(MatDialog);
  
  departments: Department[] = [];
  displayedColumns: string[] = ['id', 'name', 'description', 'actions'];
  loading = true;

  ngOnInit() {
    this.loadDepartments();
  }

  loadDepartments() {
    this.loading = true;
    this.departmentService.getAll().subscribe({
      next: (data: Department[]) => {
        this.departments = data;
        this.loading = false;
      },
      error: (err: any) => {
        console.error('Failed to load departments', err);
        this.loading = false;
      }
    });
  }

  openDialog(department?: Department) {
    const dialogRef = this.dialog.open(DepartmentDialogComponent, {
      width: '400px',
      data: department || null
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        if (department?.id) {
          this.departmentService.update(department.id, result).subscribe(() => this.loadDepartments());
        } else {
          this.departmentService.create(result).subscribe(() => this.loadDepartments());
        }
      }
    });
  }

  assignOfficer(department: Department) {
    const dialogRef = this.dialog.open(AssignOfficerDialogComponent, {
      width: '400px',
      data: department
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.departmentService.assignOfficer(department.id, result.officerId).subscribe(() => {
          alert('Officer assigned successfully!');
          this.loadDepartments();
        });
      }
    });
  }

  deleteDepartment(id: number) {
    if (confirm('Are you sure you want to delete this department?')) {
      this.departmentService.delete(id).subscribe(() => this.loadDepartments());
    }
  }
}
