import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../core/services/auth.service';
import { UserService } from '../../core/services/user.service';
import { User } from '../../core/models/models';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatDividerModule,
    MatSlideToggleModule,
    MatSnackBarModule,
    FormsModule
  ],
  template: `
    <div class="profile-wrapper" *ngIf="user">
      <div class="header">
        <h1 class="mat-h1">My Profile</h1>
        <p class="subtitle">Manage your personal information and settings.</p>
      </div>

      <div class="profile-grid">
        <!-- Main Profile Info -->
        <mat-card class="gms-card profile-card">
          <mat-card-header>
            <div mat-card-avatar class="avatar-placeholder" *ngIf="!user.profileImageUrl">
              {{ user.fullName.charAt(0) | uppercase }}
            </div>
            <img mat-card-avatar [src]="getImageUrl(user.profileImageUrl)" *ngIf="user.profileImageUrl" class="profile-image" />
            <mat-card-title>{{ user.fullName }}</mat-card-title>
            <mat-card-subtitle>{{ user.role }} Account</mat-card-subtitle>
          </mat-card-header>
          
          <mat-card-content class="mt-4">
            <div class="flex gap-4 mb-4">
               <button mat-stroked-button color="primary" (click)="fileInput.click()" [disabled]="uploading">
                 <mat-icon>upload</mat-icon> {{ uploading ? 'Uploading...' : 'Upload Image' }}
               </button>
               <input type="file" #fileInput (change)="onFileSelected($event)" accept="image/*" class="hidden">
            </div>

            <div class="info-row">
              <mat-icon>email</mat-icon>
              <div class="info-content">
                <span class="label">Email Address</span>
                <span class="value">{{ user.email }}</span>
              </div>
            </div>
            <mat-divider class="my-3"></mat-divider>
            <div class="info-row">
              <mat-icon>badge</mat-icon>
              <div class="info-content">
                <span class="label">Role / Access Level</span>
                <span class="value">{{ user.role }}</span>
              </div>
            </div>
            <mat-divider class="my-3"></mat-divider>
            <div class="info-row">
              <mat-icon>account_circle</mat-icon>
              <div class="info-content">
                <span class="label">Account Status</span>
                <span class="value text-success">Active</span>
              </div>
            </div>
          </mat-card-content>
          
          <mat-card-actions align="end" class="px-4 pb-4">
            <button mat-stroked-button color="warn" (click)="logout()">
              <mat-icon>logout</mat-icon> Sign Out
            </button>
          </mat-card-actions>
        </mat-card>

        <!-- Settings Placeholder -->
        <mat-card class="gms-card settings-card">
          <mat-card-header>
            <mat-icon mat-card-avatar>settings</mat-icon>
            <mat-card-title>Preferences</mat-card-title>
          </mat-card-header>
          <mat-card-content class="mt-4">
            <p class="text-secondary">Manage your notification preferences and account settings.</p>
            
            <div class="setting-item">
              <div class="setting-text">
                <span class="setting-title">Email Notifications</span>
                <span class="setting-desc">Receive updates about your grievances</span>
              </div>
              <mat-slide-toggle 
                [(ngModel)]="emailNotificationsEnabled" 
                (change)="onPreferenceChange()"
                [disabled]="savingPreferences">
              </mat-slide-toggle>
            </div>
            
          </mat-card-content>
        </mat-card>
      </div>
    </div>
  `,
  styles: [`
    .profile-wrapper {
      max-width: 1000px;
      margin: 0 auto;
    }
    
    .header {
      margin-bottom: 2rem;
    }
    
    .profile-grid {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 2rem;
    }

    .avatar-placeholder {
      background: var(--primary-color);
      color: white;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 1.5rem;
      font-weight: bold;
      border-radius: 50%;
    }

    .profile-image {
      width: 40px;
      height: 40px;
      border-radius: 50%;
      object-fit: cover;
      border: 1px solid var(--border-color);
    }

    .hidden {
      display: none;
    }

    .info-row {
      display: flex;
      align-items: flex-start;
      gap: 16px;
      padding: 8px 0;
    }

    .info-row mat-icon {
      color: var(--text-secondary);
      margin-top: 4px;
    }

    .info-content {
      display: flex;
      flex-direction: column;
    }

    .info-content .label {
      font-size: 0.85rem;
      color: var(--text-secondary);
      margin-bottom: 2px;
    }

    .info-content .value {
      font-weight: 500;
      color: var(--text-primary);
    }

    .text-success {
      color: #10b981 !important;
    }

    .setting-item {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 16px 0;
      border-top: 1px solid var(--border-color);
      margin-top: 16px;
    }

    .setting-text {
      display: flex;
      flex-direction: column;
    }

    .setting-title {
      font-weight: 500;
    }

    .setting-desc {
      font-size: 0.85rem;
      color: var(--text-secondary);
    }

    @media (max-width: 768px) {
      .profile-grid {
        grid-template-columns: 1fr;
      }
    }
  `]
})
export class ProfileComponent implements OnInit {
  private authService = inject(AuthService);
  private userService = inject(UserService);
  private snackBar = inject(MatSnackBar);
  
  user: User | null = null;
  uploading = false;
  savingPreferences = false;
  emailNotificationsEnabled = true;

  ngOnInit() {
    this.user = this.authService.currentUser;
    if (this.user && this.user.emailNotificationsEnabled !== undefined) {
      this.emailNotificationsEnabled = this.user.emailNotificationsEnabled;
    }
  }

  getImageUrl(url: string | undefined): string {
    if (!url) return '';
    if (url.startsWith('http')) return url;
    return `${environment.apiUrl.replace('/api', '')}${url}`;
  }

  onFileSelected(event: any) {
    const file: File = event.target.files[0];
    if (file) {
      this.uploading = true;
      this.userService.uploadProfileImage(file).subscribe({
        next: (res) => {
          this.uploading = false;
          if (this.user) {
            this.user.profileImageUrl = res.profileImageUrl;
            // Update auth service's internal state to reflect the change everywhere
            this.authService.updateUser(this.user);
          }
          this.snackBar.open('Profile image updated successfully', 'Close', { duration: 3000 });
        },
        error: () => {
          this.uploading = false;
          this.snackBar.open('Failed to update profile image', 'Close', { duration: 3000 });
        }
      });
    }
  }

  onPreferenceChange() {
    this.savingPreferences = true;
    this.userService.updatePreferences(this.emailNotificationsEnabled).subscribe({
      next: () => {
        this.savingPreferences = false;
        if (this.user) {
          this.user.emailNotificationsEnabled = this.emailNotificationsEnabled;
          this.authService.updateUser(this.user);
        }
        this.snackBar.open('Preferences saved', 'Close', { duration: 2000 });
      },
      error: () => {
        this.savingPreferences = false;
        // Revert change
        this.emailNotificationsEnabled = !this.emailNotificationsEnabled;
        this.snackBar.open('Failed to save preferences', 'Close', { duration: 3000 });
      }
    });
  }

  logout() {
    this.authService.logout();
  }
}
