import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { SkeletonComponent } from '../skeleton/skeleton.component';

@Component({
  selector: 'app-stat-card',
  standalone: true,
  imports: [CommonModule, MatIconModule, SkeletonComponent],
  template: `
    <div class="stat-card" [ngClass]="colorClass">
      <div class="stat-icon-wrapper">
        <mat-icon>{{ icon }}</mat-icon>
      </div>
      <div class="stat-content">
        <h3 class="stat-title">{{ title }}</h3>
        
        <ng-container *ngIf="!loading; else loadingTemplate">
          <div class="stat-value-row">
            <span class="stat-value">{{ value }}</span>
            <span class="stat-trend" *ngIf="trend" [ngClass]="trendType">
              <mat-icon>{{ trendType === 'up' ? 'trending_up' : (trendType === 'down' ? 'trending_down' : 'trending_flat') }}</mat-icon>
              {{ trend }}
            </span>
          </div>
          <p class="stat-desc" *ngIf="description">{{ description }}</p>
        </ng-container>
        
        <ng-template #loadingTemplate>
          <app-skeleton type="text" width="60px" height="28px"></app-skeleton>
          <app-skeleton *ngIf="description" type="text" width="100px" height="14px"></app-skeleton>
        </ng-template>
      </div>
    </div>
  `,
  styles: [`
    .stat-card {
      background: white;
      border-radius: 8px;
      padding: 1.5rem;
      display: flex;
      gap: 1.5rem;
      align-items: flex-start;
      border: 1px solid var(--border-color);
      transition: box-shadow 0.2s, transform 0.2s;
      position: relative;
      overflow: hidden;
    }
    
    .stat-card:hover {
      box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06);
      transform: translateY(-2px);
    }
    
    /* Subtle left border accent like Azure */
    .stat-card::before {
      content: '';
      position: absolute;
      left: 0;
      top: 0;
      bottom: 0;
      width: 4px;
      background: var(--primary-color);
    }
    .stat-card.blue::before { background: #3b82f6; }
    .stat-card.green::before { background: #10b981; }
    .stat-card.orange::before { background: #f97316; }
    .stat-card.red::before { background: #ef4444; }
    .stat-card.purple::before { background: #8b5cf6; }

    .stat-icon-wrapper {
      width: 48px;
      height: 48px;
      border-radius: 8px;
      display: flex;
      align-items: center;
      justify-content: center;
      background: rgba(0,0,0,0.03);
    }
    .stat-card.blue .stat-icon-wrapper { background: rgba(59, 130, 246, 0.1); color: #3b82f6; }
    .stat-card.green .stat-icon-wrapper { background: rgba(16, 185, 129, 0.1); color: #10b981; }
    .stat-card.orange .stat-icon-wrapper { background: rgba(249, 115, 22, 0.1); color: #f97316; }
    .stat-card.red .stat-icon-wrapper { background: rgba(239, 68, 68, 0.1); color: #ef4444; }
    .stat-card.purple .stat-icon-wrapper { background: rgba(139, 92, 246, 0.1); color: #8b5cf6; }

    .stat-content {
      flex: 1;
    }
    .stat-title {
      margin: 0 0 0.5rem 0;
      font-size: 0.85rem;
      font-weight: 600;
      color: var(--text-secondary);
      text-transform: uppercase;
      letter-spacing: 0.05em;
    }
    .stat-value-row {
      display: flex;
      align-items: baseline;
      gap: 12px;
      margin-bottom: 0.25rem;
    }
    .stat-value {
      font-size: 1.8rem;
      font-weight: 700;
      color: var(--text-primary);
    }
    .stat-trend {
      display: flex;
      align-items: center;
      font-size: 0.85rem;
      font-weight: 500;
    }
    .stat-trend mat-icon {
      font-size: 16px;
      width: 16px;
      height: 16px;
      margin-right: 2px;
    }
    .stat-trend.up { color: #10b981; }
    .stat-trend.down { color: #ef4444; }
    .stat-trend.neutral { color: #64748b; }

    .stat-desc {
      margin: 0;
      font-size: 0.85rem;
      color: var(--text-secondary);
    }
  `]
})
export class StatCardComponent {
  @Input() title: string = '';
  @Input() value: number | string = 0;
  @Input() icon: string = 'analytics';
  @Input() description?: string;
  @Input() trend?: string;
  @Input() trendType: 'up' | 'down' | 'neutral' = 'neutral';
  @Input() colorClass: 'blue' | 'green' | 'orange' | 'red' | 'purple' = 'blue';
  @Input() loading: boolean = false;
}
