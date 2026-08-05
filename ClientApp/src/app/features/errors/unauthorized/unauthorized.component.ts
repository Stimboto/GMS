import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-unauthorized',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="unauthorized-container">
      <h2>Unauthorized</h2>
      <p>You do not have permission to view this page.</p>
    </div>
  `,
  styles: [`
    .unauthorized-container { padding: 20px; text-align: center; color: red; }
  `]
})
export class UnauthorizedComponent {}
