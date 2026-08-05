import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-empty-state',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatButtonModule],
  template: `
    <div class="empty-state">
      <div class="icon-wrapper">
        <mat-icon class="empty-icon">{{ icon }}</mat-icon>
      </div>
      <h3>{{ title }}</h3>
      <p>{{ description }}</p>
      <button *ngIf="actionLabel" mat-flat-button color="primary" class="action-btn" (click)="onAction.emit()">
        {{ actionLabel }}
      </button>
    </div>
  `,
  styles: [`
    .empty-state {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: 5rem 2rem;
      text-align: center;
      background: white;
      border-radius: 1.5rem;
      border: 1px solid rgba(0, 0, 0, 0.05);
      box-shadow: 0 10px 25px -5px rgba(0, 0, 0, 0.02), 0 8px 10px -6px rgba(0, 0, 0, 0.01);
      margin: 2rem auto;
      max-width: 500px;
      transition: transform 0.3s ease;
    }
    .empty-state:hover {
      transform: translateY(-2px);
    }
    .icon-wrapper {
      width: 80px;
      height: 80px;
      background: linear-gradient(135deg, #f0f9ff 0%, #e0f2fe 100%);
      border-radius: 50%;
      display: flex;
      align-items: center;
      justify-content: center;
      margin-bottom: 1.5rem;
      box-shadow: 0 4px 14px 0 rgba(14, 165, 233, 0.2);
    }
    .empty-icon {
      font-size: 40px;
      width: 40px;
      height: 40px;
      color: #0ea5e9;
    }
    h3 {
      font-size: 1.5rem;
      font-weight: 700;
      color: #0f172a;
      margin: 0 0 0.5rem 0;
      letter-spacing: -0.025em;
    }
    p {
      color: #64748b;
      font-size: 1.05rem;
      margin: 0 0 2rem 0;
      line-height: 1.5;
    }
    .action-btn {
      padding: 0 2rem;
      border-radius: 9999px;
      height: 44px;
      font-weight: 600;
      letter-spacing: 0.025em;
      box-shadow: 0 4px 6px -1px rgba(59, 130, 246, 0.3);
    }
  `]
})
export class EmptyStateComponent {
  @Input() icon: string = 'inbox';
  @Input() title: string = 'No Data Found';
  @Input() description: string = 'There is nothing to display here yet.';
  @Input() actionLabel: string = '';
  @Output() onAction = new EventEmitter<void>();
}
