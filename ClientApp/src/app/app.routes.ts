import { Routes } from '@angular/router';
import { HomeComponent } from './features/home/home.component';
import { LoginComponent } from './features/auth/login/login.component';
import { RegisterComponent } from './features/auth/register/register.component';
import { PrivacyComponent } from './features/privacy/privacy.component';
import { TermsComponent } from './features/terms/terms.component';
import { UnauthorizedComponent } from './features/errors/unauthorized/unauthorized.component';
import { NotFoundComponent } from './features/errors/not-found/not-found.component';
import { CreateGrievanceComponent } from './features/grievance/create-grievance/create-grievance.component';
import { GrievanceListComponent } from './features/grievance/grievance-list/grievance-list.component';
import { GrievanceDetailComponent } from './features/grievance/grievance-detail/grievance-detail.component';
import { NotificationsComponent } from './features/notifications/notifications.component';
import { ProfileComponent } from './features/profile/profile.component';
import { SharedLayoutComponent } from './layout/shared-layout/shared-layout.component';

// Dashboards
import { CitizenDashboardComponent } from './features/dashboard/citizen-dashboard/citizen-dashboard.component';
import { OfficerDashboardComponent } from './features/dashboard/officer-dashboard/officer-dashboard.component';
import { AdminDashboardComponent } from './features/dashboard/admin-dashboard/admin-dashboard.component';

// Admin
import { UsersComponent } from './features/admin/users/users.component';
import { DepartmentsComponent } from './features/admin/departments/departments.component';
import { GrievancesComponent as AdminGrievancesComponent } from './features/admin/grievances/grievances.component';

import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';

export const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { path: 'privacy', component: PrivacyComponent },
  { path: 'terms', component: TermsComponent },
  { path: 'unauthorized', component: UnauthorizedComponent },
  { 
    path: '', 
    component: SharedLayoutComponent,
    canActivate: [authGuard],
    children: [
      // Citizen Routes
      { 
        path: 'citizen/dashboard', 
        component: CitizenDashboardComponent,
        canActivate: [roleGuard],
        data: { roles: ['Citizen'] }
      },
      { 
        path: 'citizen/grievances', 
        component: GrievanceListComponent,
        canActivate: [roleGuard],
        data: { roles: ['Citizen'] }
      },
      { 
        path: 'citizen/grievances/create', 
        component: CreateGrievanceComponent,
        canActivate: [roleGuard],
        data: { roles: ['Citizen'] }
      },
      { 
        path: 'citizen/grievances/:id', 
        component: GrievanceDetailComponent,
        canActivate: [roleGuard],
        data: { roles: ['Citizen'] }
      },

      // Officer Routes
      { 
        path: 'officer/dashboard', 
        component: OfficerDashboardComponent,
        canActivate: [roleGuard],
        data: { roles: ['Officer'] }
      },
      { 
        path: 'officer/grievances', 
        component: GrievanceListComponent,
        canActivate: [roleGuard],
        data: { roles: ['Officer'] }
      },
      { 
        path: 'officer/grievances/:id', 
        component: GrievanceDetailComponent,
        canActivate: [roleGuard],
        data: { roles: ['Officer'] }
      },

      // Admin Routes
      { 
        path: 'admin/dashboard', 
        component: AdminDashboardComponent,
        canActivate: [roleGuard],
        data: { roles: ['Admin'] }
      },
      { 
        path: 'admin/users', 
        component: UsersComponent,
        canActivate: [roleGuard],
        data: { roles: ['Admin'] }
      },
      { 
        path: 'admin/departments', 
        component: DepartmentsComponent,
        canActivate: [roleGuard],
        data: { roles: ['Admin'] }
      },
      { 
        path: 'admin/grievances', 
        component: GrievanceListComponent,
        canActivate: [roleGuard],
        data: { roles: ['Admin'] }
      },
      { 
        path: 'admin/grievances/:id', 
        component: GrievanceDetailComponent,
        canActivate: [roleGuard],
        data: { roles: ['Admin'] }
      },

      // Shared Protected Routes
      { path: 'notifications', component: NotificationsComponent },
      { path: 'profile', component: ProfileComponent }
    ]
  },
  { path: '**', component: NotFoundComponent }
];
