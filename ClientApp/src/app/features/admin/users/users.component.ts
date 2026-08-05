import { Component, OnInit, ViewChild, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatMenuModule } from '@angular/material/menu';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { FormsModule } from '@angular/forms';
import { UserService, UserDto } from '../../../core/services/user.service';
import { RoleChangeDialogComponent } from './role-change-dialog/role-change-dialog.component';
import { UserProfileDialogComponent } from './user-profile-dialog/user-profile-dialog.component';

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatButtonModule,
    MatIconModule,
    MatInputModule,
    MatMenuModule,
    MatDialogModule,
    MatSnackBarModule,
    FormsModule
  ],
  templateUrl: './users.component.html',
  styleUrls: ['./users.component.css']
})
export class UsersComponent implements OnInit {
  private userService = inject(UserService);
  private dialog = inject(MatDialog);
  private snackBar = inject(MatSnackBar);

  displayedColumns: string[] = ['avatar', 'fullName', 'email', 'role', 'status', 'createdAt', 'actions'];
  dataSource = new MatTableDataSource<UserDto>();
  
  loading = true;
  searchQuery = '';

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.loading = true;
    this.userService.getAllUsers().subscribe({
      next: (users) => {
        this.dataSource.data = users;
        this.dataSource.paginator = this.paginator;
        this.dataSource.sort = this.sort;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.snackBar.open('Failed to load users', 'Close', { duration: 3000 });
      }
    });
  }

  applyFilter(): void {
    this.dataSource.filter = this.searchQuery.trim().toLowerCase();
  }

  openRoleChangeDialog(user: UserDto): void {
    const dialogRef = this.dialog.open(RoleChangeDialogComponent, {
      width: '400px',
      data: { user }
    });

    dialogRef.afterClosed().subscribe(newRole => {
      if (newRole && newRole !== user.role) {
        this.userService.updateUserRole(user.id, newRole).subscribe({
          next: () => {
            this.snackBar.open('Role updated successfully', 'Close', { duration: 3000 });
            this.loadUsers();
          },
          error: (err) => {
            this.snackBar.open(err.error?.message || 'Failed to update role', 'Close', { duration: 3000 });
          }
        });
      }
    });
  }

  toggleUserStatus(user: UserDto): void {
    const newStatus = !user.isActive;
    this.userService.updateUserStatus(user.id, newStatus).subscribe({
      next: () => {
        this.snackBar.open(`User ${newStatus ? 'activated' : 'deactivated'} successfully`, 'Close', { duration: 3000 });
        this.loadUsers();
      },
      error: (err) => {
        this.snackBar.open(err.error?.message || 'Failed to update status', 'Close', { duration: 3000 });
      }
    });
  }

  deleteUser(user: UserDto): void {
    if (confirm(`Are you sure you want to delete ${user.fullName}?`)) {
      this.userService.deleteUser(user.id).subscribe({
        next: () => {
          this.snackBar.open('User deleted successfully', 'Close', { duration: 3000 });
          this.loadUsers();
        },
        error: (err) => {
          this.snackBar.open(err.error?.message || 'Failed to delete user', 'Close', { duration: 3000 });
        }
      });
    }
  }

  viewProfile(user: UserDto): void {
    this.dialog.open(UserProfileDialogComponent, {
      width: '800px',
      data: { user }
    });
  }

  getRoleBadgeClass(role: string): string {
    switch (role) {
      case 'Citizen': return 'bg-blue-100 text-blue-800';
      case 'Officer': return 'bg-orange-100 text-orange-800';
      case 'Admin': return 'bg-purple-100 text-purple-800';
      default: return 'bg-gray-100 text-gray-800';
    }
  }

  getStatusBadgeClass(isActive: boolean): string {
    return isActive ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800';
  }
}
