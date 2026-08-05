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
  selector: 'app-citizen-dashboard',
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
  templateUrl: './citizen-dashboard.component.html',
  styleUrls: ['./citizen-dashboard.component.css']
})
export class CitizenDashboardComponent implements OnInit {
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
    this.dashboardService.getCitizenDashboard().subscribe({
      next: (res: DashboardStats) => {
        this.stats = res;
        this.loadingStats = false;
      },
      error: () => this.loadingStats = false
    });

    this.grievanceService.getMyGrievances().subscribe({
      next: (res: Grievance[]) => {
        // Take top 3 most recent
        this.recentGrievances = res.sort((a: Grievance, b: Grievance) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()).slice(0, 3);
        this.loadingRecent = false;
      },
      error: () => this.loadingRecent = false
    });
  }
}
