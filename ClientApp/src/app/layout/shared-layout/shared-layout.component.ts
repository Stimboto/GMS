import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, RouterOutlet } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatBadgeModule } from '@angular/material/badge';
import { MatDividerModule } from '@angular/material/divider';
import { ThemeService } from '../../core/services/theme.service';
import { AuthService } from '../../core/services/auth.service';
import { NotificationService } from '../../core/services/notification.service';
import { environment } from '../../../environments/environment';
import { Router } from '@angular/router';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { User } from '../../core/models/models';
import { SignalRService } from '../../core/services/signalr.service';

interface NavItem {
  path: string;
  icon: string;
  label: string;
  roles: string[];
}

@Component({
  selector: 'app-shared-layout',
  standalone: true,
  imports: [
    CommonModule, 
    RouterModule, 
    RouterOutlet,
    MatToolbarModule,
    MatIconModule,
    MatButtonModule,
    MatMenuModule,
    MatBadgeModule,
    MatDividerModule,
    MatSnackBarModule
  ],
  templateUrl: './shared-layout.component.html',
  styleUrls: ['./shared-layout.component.css']
})
export class SharedLayoutComponent implements OnInit {
  themeService = inject(ThemeService);
  authService = inject(AuthService);
  notificationService = inject(NotificationService);
  signalRService = inject(SignalRService);
  snackBar = inject(MatSnackBar);
  router = inject(Router);

  isSidebarCollapsed = false;
  unreadNotifications = 0;
  user: User | null = null;

  navItems: NavItem[] = [
    // Citizen
    { path: '/citizen/dashboard', icon: 'dashboard', label: 'Dashboard', roles: ['Citizen'] },
    { path: '/citizen/grievances', icon: 'list_alt', label: 'My Grievances', roles: ['Citizen'] },
    { path: '/citizen/grievances/create', icon: 'add_circle', label: 'Create Grievance', roles: ['Citizen'] },
    
    // Officer
    { path: '/officer/dashboard', icon: 'dashboard', label: 'Dashboard', roles: ['Officer'] },
    { path: '/officer/grievances', icon: 'assignment', label: 'Assigned Cases', roles: ['Officer'] },
    
    // Admin
    { path: '/admin/dashboard', icon: 'dashboard', label: 'Dashboard', roles: ['Admin'] },
    { path: '/admin/users', icon: 'people', label: 'User Management', roles: ['Admin'] },
    { path: '/admin/departments', icon: 'business', label: 'Departments', roles: ['Admin'] },
    { path: '/admin/grievances', icon: 'list_alt', label: 'Grievances', roles: ['Admin'] },
    
    // Shared
    { path: '/notifications', icon: 'notifications', label: 'Notifications', roles: ['Citizen', 'Officer', 'Admin'] },
    { path: '/profile', icon: 'person', label: 'Profile', roles: ['Citizen', 'Officer', 'Admin'] }
  ];

  filteredNavItems: NavItem[] = [];

  ngOnInit() {
    this.authService.currentUser$.subscribe(u => {
      this.user = u;
      if (this.user) {
        this.filteredNavItems = this.navItems.filter(item => item.roles.includes(this.user!.role));
      } else {
        this.filteredNavItems = [];
      }
    });

    this.notificationService.getUnreadCount().subscribe(count => {
      this.unreadNotifications = count;
    });

    this.signalRService.startConnection();
    this.signalRService.notificationReceived.subscribe(data => {
      // Increment badge
      this.unreadNotifications++;
      // Show toast
      this.snackBar.open(`${data.title}: ${data.message}`, 'Close', {
        duration: 5000,
        horizontalPosition: 'end',
        verticalPosition: 'bottom'
      });
    });
  }

  toggleSidebar() {
    this.isSidebarCollapsed = !this.isSidebarCollapsed;
  }

  toggleTheme() {
    this.themeService.toggleTheme();
  }

  getImageUrl(url: string | undefined): string {
    if (!url) return '';
    if (url.startsWith('http')) return url;
    return `${environment.apiUrl.replace('/api', '')}${url}`;
  }

  logout() {
    this.signalRService.stopConnection();
    this.authService.logout();
  }
}
