import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state.component';

@Component({
  selector: 'app-departments',
  standalone: true,
  imports: [CommonModule, EmptyStateComponent],
  template: `
    <div class="p-4 md:p-8 w-full min-h-full">
      <div class="mb-6">
        <h1 class="text-3xl font-bold text-gray-800">Department Management</h1>
        <p class="text-gray-600">Configure system departments and assign officers.</p>
      </div>

      <app-empty-state 
        icon="construction"
        title="Coming Soon"
        description="The backend APIs for Department Management are not yet available.">
      </app-empty-state>
    </div>
  `,
  styles: [`
    .p-6 { padding: 1.5rem; }
    .mb-6 { margin-bottom: 1.5rem; }
    .max-w-7xl { max-width: 80rem; }
    .mx-auto { margin-left: auto; margin-right: auto; }
    .text-3xl { font-size: 1.875rem; line-height: 2.25rem; }
    .font-bold { font-weight: 700; }
    .text-gray-800 { color: #1f2937; }
    .text-gray-600 { color: #4b5563; margin-top: 0.5rem; }
  `]
})
export class DepartmentsComponent {}
