import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatRadioModule } from '@angular/material/radio';
import { FormsModule } from '@angular/forms';
import { UserDto } from '../../../../core/services/user.service';

@Component({
  selector: 'app-role-change-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule, MatRadioModule, FormsModule],
  template: `
    <h2 mat-dialog-title>Change User Role</h2>
    <mat-dialog-content>
      <p class="mb-4 text-gray-600">
        You are changing the role for <strong>{{data.user.fullName}}</strong> ({{data.user.email}}).
      </p>
      
      <div class="bg-blue-50 border border-blue-100 p-4 rounded-lg mb-6 flex items-start gap-3">
        <span class="material-icons text-blue-500">info</span>
        <p class="text-sm text-blue-800 m-0">
          Role change becomes effective after the user's next login.
        </p>
      </div>

      <mat-radio-group [(ngModel)]="selectedRole" class="flex flex-col gap-3">
        <mat-radio-button value="Citizen" [checked]="selectedRole === 'Citizen'">
          Citizen
          <p class="text-xs text-gray-500 m-0 pl-7">Can submit and track their own grievances.</p>
        </mat-radio-button>
        <mat-radio-button value="Officer" [checked]="selectedRole === 'Officer'">
          Officer
          <p class="text-xs text-gray-500 m-0 pl-7">Can review and resolve assigned grievances.</p>
        </mat-radio-button>
        <mat-radio-button value="Admin" [checked]="selectedRole === 'Admin'">
          Admin
          <p class="text-xs text-gray-500 m-0 pl-7">Full system access including user management.</p>
        </mat-radio-button>
      </mat-radio-group>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="onNoClick()">Cancel</button>
      <button mat-flat-button color="primary" [mat-dialog-close]="selectedRole" [disabled]="selectedRole === data.user.role">
        Confirm Change
      </button>
    </mat-dialog-actions>
  `
})
export class RoleChangeDialogComponent {
  selectedRole: string;

  constructor(
    public dialogRef: MatDialogRef<RoleChangeDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { user: UserDto }
  ) {
    this.selectedRole = data.user.role;
  }

  onNoClick(): void {
    this.dialogRef.close();
  }
}
