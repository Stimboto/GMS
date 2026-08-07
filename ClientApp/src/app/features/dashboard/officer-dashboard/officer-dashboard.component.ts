import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { DashboardService } from '../../../core/services/dashboard.service';
import { GrievanceService } from '../../../core/services/grievance.service';
import { DashboardStats, Grievance } from '../../../core/models/models';
import { StatCardComponent } from '../../../shared/components/stat-card/stat-card.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { SkeletonComponent } from '../../../shared/components/skeleton/skeleton.component';
import { SignalRService } from '../../../core/services/signalr.service';

@Component({
  selector: 'app-officer-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatButtonModule,
    MatIconModule,
    StatCardComponent,
    EmptyStateComponent,
    SkeletonComponent
  ],
  template: `
    <div class="dashboard-header">
      <div>
        <h2>Officer Dashboard</h2>
        <p>Manage and resolve grievances assigned to your department.</p>
      </div>
      <button mat-flat-button color="primary" routerLink="/officer/grievances">
        View All Assigned
      </button>
    </div>

    <!-- Stats Grid -->
    <div class="stats-grid">
      <app-stat-card 
        title="Total Assigned" 
        [value]="stats?.totalGrievances || 0" 
        icon="assignment"
        colorClass="blue"
        description="Total cases for you"
        [loading]="loadingStats">
      </app-stat-card>

      <app-stat-card 
        title="Pending Action" 
        [value]="stats?.pendingGrievances || 0" 
        icon="warning"
        colorClass="orange"
        description="Requires your attention"
        [loading]="loadingStats">
      </app-stat-card>

      <app-stat-card 
        title="Resolved" 
        [value]="stats?.resolvedGrievances || 0" 
        icon="task_alt"
        colorClass="green"
        description="Successfully closed by you"
        [loading]="loadingStats">
      </app-stat-card>
    </div>

    <!-- Recent Assignments Section -->
    <div class="recent-section">
      <div class="section-header">
        <h3>Recent Assignments</h3>
        <a routerLink="/officer/grievances" class="view-all">View All</a>
      </div>

      <ng-container *ngIf="loadingRecent">
        <div class="grid-layout">
          <app-skeleton type="card" height="150px"></app-skeleton>
          <app-skeleton type="card" height="150px"></app-skeleton>
          <app-skeleton type="card" height="150px"></app-skeleton>
        </div>
      </ng-container>

      <ng-container *ngIf="!loadingRecent && recentGrievances.length > 0">
        <div class="grid-layout">
          <div class="grievance-card" *ngFor="let g of recentGrievances" [routerLink]="['/officer/grievances', g.id]">
            <div class="card-header">
              <span class="status-badge" [ngClass]="g.status.replace(' ', '-').toLowerCase()">{{ g.status }}</span>
              <span class="date">{{ g.createdAt | date:'shortDate' }}</span>
            </div>
            <h4 class="title">{{ g.title }}</h4>
            <p class="desc">{{ g.description }}</p>
            <div class="card-footer">
                    <span class="text-xs text-gray-500 flex items-center gap-1"><mat-icon>person</mat-icon> {{ g.submittedBy || 'Unknown Citizen' }}</span>
              <span class="priority" [ngClass]="g.priority.toLowerCase()">{{ g.priority }}</span>
            </div>
          </div>
        </div>
      </ng-container>

      <ng-container *ngIf="!loadingRecent && recentGrievances.length === 0">
        <app-empty-state 
          icon="check_circle_outline"
          title="All caught up!"
          description="You don't have any grievances assigned to you right now."
          actionLabel="Refresh"
          (onAction)="loadData()">
        </app-empty-state>
      </ng-container>
    </div>
  `,
  styleUrls: ['../citizen-dashboard/citizen-dashboard.component.css'] // Reuse same styles
})
export class OfficerDashboardComponent implements OnInit {
  private dashboardService = inject(DashboardService);
  private grievanceService = inject(GrievanceService);

  stats: DashboardStats | null = null;
  recentGrievances: Grievance[] = [];
  loadingStats = true;
  loadingRecent = true;
  private signalRService = inject(SignalRService);

  ngOnInit() {
    this.loadData();
    this.signalRService.notificationReceived.subscribe(() => {
      this.loadData();
    });
  }

  loadData() {
    this.loadingStats = true;
    this.loadingRecent = true;
    
    this.dashboardService.getOfficerDashboard().subscribe({
      next: (res: DashboardStats) => {
        this.stats = res;
        this.loadingStats = false;
      },
      error: () => this.loadingStats = false
    });

    this.grievanceService.getAssigned().subscribe({
      next: (res: Grievance[]) => {
        // Take top 3 most recent
        this.recentGrievances = res.sort((a: Grievance, b: Grievance) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()).slice(0, 3);
        this.loadingRecent = false;
      },
      error: () => this.loadingRecent = false
    });
  }
}
