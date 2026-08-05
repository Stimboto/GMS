import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTabsModule } from '@angular/material/tabs';
import { BaseChartDirective } from 'ng2-charts';
import { DashboardService } from '../../../core/services/dashboard.service';
import { GrievanceService } from '../../../core/services/grievance.service';
import { DashboardStats, Grievance, ChartData, MonthlyGrievance } from '../../../core/models/models';
import { StatCardComponent } from '../../../shared/components/stat-card/stat-card.component';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { SkeletonComponent } from '../../../shared/components/skeleton/skeleton.component';
import { SignalRService } from '../../../core/services/signalr.service';
import { ChartConfiguration, ChartOptions, ChartType } from 'chart.js';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatButtonModule,
    MatIconModule,
    MatTabsModule,
    BaseChartDirective,
    StatCardComponent,
    EmptyStateComponent,
    SkeletonComponent
  ],
  templateUrl: './admin-dashboard.component.html',
  styleUrls: ['./admin-dashboard.component.css']
})
export class AdminDashboardComponent implements OnInit {
  private dashboardService = inject(DashboardService);
  private grievanceService = inject(GrievanceService);

  stats: DashboardStats | null = null;
  recentGrievances: Grievance[] = [];
  loadingStats = true;
  loadingRecent = true;

  // Chart configuration
  public statusChartType: ChartType = 'pie';
  public statusChartOptions: ChartOptions = { responsive: true, maintainAspectRatio: false };
  public statusChartData: ChartConfiguration['data'] = { labels: [], datasets: [{ data: [] }] };

  public deptChartType: ChartType = 'bar';
  public deptChartOptions: ChartOptions = { responsive: true, maintainAspectRatio: false };
  public deptChartData: ChartConfiguration['data'] = { labels: [], datasets: [{ data: [], label: 'Grievances' }] };

  public monthlyChartType: ChartType = 'line';
  public monthlyChartOptions: ChartOptions = { responsive: true, maintainAspectRatio: false };
  public monthlyChartData: ChartConfiguration['data'] = { labels: [], datasets: [{ data: [], label: 'New Grievances' }] };

  private signalRService = inject(SignalRService);

  ngOnInit() {
    this.loadData();
    this.loadCharts();
    this.signalRService.notificationReceived.subscribe(() => {
      this.loadData();
      this.loadCharts();
    });
  }

  loadData() {
    this.loadingStats = true;
    this.loadingRecent = true;
    
    this.dashboardService.getAdminDashboard().subscribe({
      next: (res: DashboardStats) => {
        this.stats = res;
        this.loadingStats = false;
      },
      error: () => this.loadingStats = false
    });

    this.grievanceService.getAll().subscribe({
      next: (res: Grievance[]) => {
        // Take top 5 most recent
        this.recentGrievances = res.sort((a: Grievance, b: Grievance) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()).slice(0, 5);
        this.loadingRecent = false;
      },
      error: () => this.loadingRecent = false
    });
  }

  loadCharts() {
    this.dashboardService.getStatusChart().subscribe(data => {
      if (data && data.labels) {
        this.statusChartData = {
          labels: data.labels,
          datasets: [{ 
            data: data.values,
            backgroundColor: ['#f59e0b', '#3b82f6', '#8b5cf6', '#10b981', '#6b7280']
          }]
        };
      }
    });

    this.dashboardService.getDepartmentChart().subscribe(data => {
      if (data && data.labels) {
        this.deptChartData = {
          labels: data.labels,
          datasets: [{ 
            data: data.values, 
            label: 'Grievances',
            backgroundColor: '#4f46e5'
          }]
        };
      }
    });

    this.dashboardService.getMonthlyChart().subscribe(data => {
      if (data) {
        this.monthlyChartData = {
          labels: data.map(d => d.month),
          datasets: [{ 
            data: data.map(d => d.count), 
            label: 'New Grievances',
            borderColor: '#ec4899',
            backgroundColor: 'rgba(236, 72, 153, 0.1)',
            fill: true,
            tension: 0.4
          }]
        };
      }
    });
  }
}
