import { Component, Inject, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { UserDto, UserService } from '../../../../core/services/user.service';
import { environment } from '../../../../../environments/environment';

@Component({
  selector: 'app-user-profile-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule, MatIconModule, MatDividerModule],
  template: `
    <div class="flex justify-between items-center px-6 py-4 border-b border-gray-100">
      <h2 class="text-xl font-semibold m-0 text-gray-800">User Profile</h2>
      <button mat-icon-button (click)="onClose()">
        <mat-icon>close</mat-icon>
      </button>
    </div>
    
    <mat-dialog-content class="!p-0 !m-0 overflow-hidden">
      <div *ngIf="loading" class="p-12 flex justify-center items-center">
        <div class="animate-pulse flex flex-col items-center gap-4">
          <div class="w-24 h-24 bg-gray-200 rounded-full"></div>
          <div class="w-48 h-6 bg-gray-200 rounded"></div>
        </div>
      </div>

      <div *ngIf="!loading && user" class="p-6">
        <!-- Header Profile Card -->
        <div class="flex items-center gap-6 mb-8">
          <div class="w-24 h-24 rounded-full bg-indigo-100 flex items-center justify-center text-indigo-700 text-3xl font-bold shadow-inner overflow-hidden border-2 border-white shadow-md">
            <ng-container *ngIf="user.profileImageUrl">
              <img [src]="getImageUrl(user.profileImageUrl)" alt="Profile" class="w-full h-full object-cover" />
            </ng-container>
            <ng-container *ngIf="!user.profileImageUrl">
              {{ user.fullName.charAt(0).toUpperCase() }}
            </ng-container>
          </div>
          <div>
            <h1 class="text-2xl font-bold text-gray-900 m-0 flex items-center gap-3">
              {{ user.fullName }}
              <span class="px-3 py-1 rounded-full text-xs font-medium" [ngClass]="getRoleBadgeClass(user.role)">
                {{user.role}}
              </span>
              <span class="px-3 py-1 rounded-full text-xs font-medium" [ngClass]="getStatusBadgeClass(user.isActive)">
                {{user.isActive ? 'Active' : 'Inactive'}}
              </span>
            </h1>
            <p class="text-gray-500 m-0 mt-1 flex items-center gap-2">
              <mat-icon class="text-[18px] w-[18px] h-[18px]">email</mat-icon>
              {{ user.email }}
            </p>
            <p class="text-gray-500 m-0 mt-1 flex items-center gap-2" *ngIf="user.phoneNumber">
              <mat-icon class="text-[18px] w-[18px] h-[18px]">phone</mat-icon>
              {{ user.phoneNumber }}
            </p>
            <p class="text-gray-400 m-0 mt-2 text-sm">Member since {{ user.createdAt | date:'longDate' }}</p>
          </div>
        </div>

        <mat-divider></mat-divider>

        <!-- Stats Grid -->
        <div class="grid grid-cols-3 gap-6 mt-8 mb-6">
          <div class="bg-gray-50 rounded-xl p-5 border border-gray-100 text-center">
            <p class="text-gray-500 text-sm font-medium mb-1">Total Grievances</p>
            <p class="text-3xl font-bold text-gray-800">{{ user.totalGrievances }}</p>
          </div>
          
          <div class="bg-green-50 rounded-xl p-5 border border-green-100 text-center">
            <p class="text-green-700 text-sm font-medium mb-1">Resolved</p>
            <p class="text-3xl font-bold text-green-800">{{ user.resolvedGrievances }}</p>
          </div>

          <div class="bg-blue-50 rounded-xl p-5 border border-blue-100 text-center" *ngIf="user.role !== 'Citizen'">
            <p class="text-blue-700 text-sm font-medium mb-1">Assigned Cases</p>
            <p class="text-3xl font-bold text-blue-800">{{ user.assignedCases }}</p>
          </div>
        </div>

        <mat-divider></mat-divider>

        <!-- Preferences Section -->
        <div class="mt-6">
          <h3 class="text-sm font-semibold text-gray-800 uppercase tracking-wide mb-4">User Preferences</h3>
          
          <div class="flex items-center justify-between p-4 bg-gray-50 rounded-xl border border-gray-100">
            <div class="flex items-center gap-3">
              <div class="w-10 h-10 rounded-full bg-indigo-100 flex items-center justify-center text-indigo-600">
                <mat-icon>notifications_active</mat-icon>
              </div>
              <div>
                <p class="font-medium text-gray-900 m-0">Email Notifications</p>
                <p class="text-sm text-gray-500 m-0">System updates and grievance status</p>
              </div>
            </div>
            <div>
              <span class="px-3 py-1 rounded-full text-xs font-medium" [ngClass]="user.emailNotificationsEnabled ? 'bg-green-100 text-green-800 border border-green-200' : 'bg-gray-100 text-gray-800 border border-gray-200'">
                {{ user.emailNotificationsEnabled ? 'Enabled' : 'Disabled' }}
              </span>
            </div>
          </div>
        </div>

      </div>
    </mat-dialog-content>
  `
})
export class UserProfileDialogComponent implements OnInit {
  private userService = inject(UserService);
  
  user: UserDto | null = null;
  loading = true;

  constructor(
    public dialogRef: MatDialogRef<UserProfileDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { user: UserDto }
  ) {
    this.user = data.user;
  }

  getImageUrl(url: string | undefined): string {
    if (!url) return '';
    if (url.startsWith('http')) return url;
    return `${environment.apiUrl.replace('/api', '')}${url}`;
  }

  ngOnInit(): void {
    // Fetch fresh user details to get stats
    this.userService.getUserById(this.data.user.id).subscribe({
      next: (u) => {
        this.user = u;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  onClose(): void {
    this.dialogRef.close();
  }

  getRoleBadgeClass(role: string): string {
    switch (role) {
      case 'Citizen': return 'bg-blue-100 text-blue-800 border border-blue-200';
      case 'Officer': return 'bg-orange-100 text-orange-800 border border-orange-200';
      case 'Admin': return 'bg-purple-100 text-purple-800 border border-purple-200';
      default: return 'bg-gray-100 text-gray-800 border border-gray-200';
    }
  }

  getStatusBadgeClass(isActive: boolean): string {
    return isActive ? 'bg-green-100 text-green-800 border border-green-200' : 'bg-red-100 text-red-800 border border-red-200';
  }
}
