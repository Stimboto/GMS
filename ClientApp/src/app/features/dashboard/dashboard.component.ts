import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { DashboardService } from '../../core/services/dashboard.service';
import { AuthService } from '../../core/services/auth.service';
import { DashboardStats, User } from '../../core/models/models';
import { BaseChartDirective } from 'ng2-charts';
import { ChartConfiguration, ChartData, ChartType } from 'chart.js';
import { provideCharts, withDefaultRegisterables } from 'ng2-charts';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatIconModule, MatButtonModule, BaseChartDirective],
  providers: [provideCharts(withDefaultRegisterables())],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit {
  private dashboardService = inject(DashboardService);
  private authService = inject(AuthService);
  
  user: User | null = null;
  stats: DashboardStats | null = null;
  loading = true;

  // Chart config
  public pieChartType: ChartType = 'pie';
  public pieChartData: ChartData<'pie', number[], string | string[]> | undefined;
  public pieChartOptions: ChartConfiguration['options'] = {
    responsive: true,
    plugins: {
      legend: {
        display: true,
        position: 'bottom',
      }
    }
  };

  ngOnInit() {
    this.user = this.authService.currentUser;
    if (this.user) {
      if (this.user.role === 'Admin') {
        this.dashboardService.getAdminDashboard().subscribe(res => {
          this.stats = res;
          this.setupCharts();
          this.loading = false;
        });
      } else if (this.user.role === 'Officer') {
        this.dashboardService.getOfficerDashboard().subscribe(res => {
          this.stats = res;
          this.setupCharts();
          this.loading = false;
        });
      } else {
        this.dashboardService.getCitizenDashboard().subscribe(res => {
          this.stats = res;
          this.setupCharts();
          this.loading = false;
        });
      }
    }
  }

  setupCharts() {
    if (this.stats) {
      this.pieChartData = {
        labels: ['Pending', 'Resolved'],
        datasets: [{
          data: [this.stats.pendingGrievances, this.stats.resolvedGrievances],
          backgroundColor: ['#f44336', '#4caf50']
        }]
      };
    }
  }
}
