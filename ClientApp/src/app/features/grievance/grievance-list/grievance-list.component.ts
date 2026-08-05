import { Component, inject, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { GrievanceService } from '../../../core/services/grievance.service';
import { AuthService } from '../../../core/services/auth.service';
import { Grievance, User } from '../../../core/models/models';
import { Router } from '@angular/router';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';
import { SkeletonComponent } from '../../../shared/components/skeleton/skeleton.component';

@Component({
  selector: 'app-grievance-list',
  standalone: true,
  imports: [
    CommonModule, 
    RouterModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    EmptyStateComponent,
    SkeletonComponent
  ],
  templateUrl: './grievance-list.component.html',
  styleUrls: ['./grievance-list.component.css']
})
export class GrievanceListComponent implements OnInit {
  private grievanceService = inject(GrievanceService);
  private authService = inject(AuthService);
  private router = inject(Router);
  
  user: User | null = null;
  displayedColumns: string[] = ['trackingId', 'title', 'category', 'priority', 'status', 'actions'];
  dataSource = new MatTableDataSource<Grievance>([]);
  loading = true;

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  ngOnInit() {
    this.user = this.authService.currentUser;
    this.loadData();
  }

  loadData() {
    this.loading = true;
    if (this.user) {
      const request$ = this.user.role === 'Citizen' 
        ? this.grievanceService.getMyGrievances() 
        : (this.user.role === 'Officer' ? this.grievanceService.getAssigned() : this.grievanceService.getAll());

      request$.subscribe({
        next: (res) => {
          this.dataSource.data = res;
          this.dataSource.paginator = this.paginator;
          this.dataSource.sort = this.sort;
          this.loading = false;
        },
        error: () => {
          this.loading = false;
        }
      });
    }
  }

  applyFilter(event: Event) {
    const filterValue = (event.target as HTMLInputElement).value;
    this.dataSource.filter = filterValue.trim().toLowerCase();

    if (this.dataSource.paginator) {
      this.dataSource.paginator.firstPage();
    }
  }

  getPriorityColor(priority: string): string {
    switch(priority?.toLowerCase()) {
      case 'high': return 'priority-high';
      case 'medium': return 'priority-medium';
      case 'low': return 'priority-low';
      default: return '';
    }
  }

  getStatusColor(status: string): string {
    switch(status?.toLowerCase()) {
      case 'pending': return 'status-pending';
      case 'in progress':
      case 'in review': return 'status-review';
      case 'resolved': return 'status-resolved';
      default: return '';
    }
  }

  viewDetails(id: number) {
    if (!this.user) return;
    const rolePath = this.user.role.toLowerCase();
    this.router.navigate([`/${rolePath}/grievances`, id]);
  }
}
