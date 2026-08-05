import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-skeleton',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="skeleton-wrapper" [ngClass]="type">
      <div class="skeleton-line" *ngIf="type === 'text'" [style.width]="width" [style.height]="height"></div>
      <div class="skeleton-circle" *ngIf="type === 'circle'" [style.width]="width" [style.height]="height"></div>
      <div class="skeleton-card" *ngIf="type === 'card'" [style.width]="width" [style.height]="height"></div>
    </div>
  `,
  styles: [`
    .skeleton-wrapper {
      display: inline-block;
      width: 100%;
    }
    .skeleton-line, .skeleton-circle, .skeleton-card {
      background: linear-gradient(90deg, #e2e8f0 25%, #f8fafc 50%, #e2e8f0 75%);
      background-size: 200% 100%;
      animation: skeleton-loading 1.5s infinite linear;
    }
    .skeleton-line {
      height: 16px;
      border-radius: 4px;
      margin-bottom: 8px;
    }
    .skeleton-circle {
      border-radius: 50%;
    }
    .skeleton-card {
      border-radius: 8px;
      height: 100px;
    }

    @keyframes skeleton-loading {
      0% {
        background-position: 200% 0;
      }
      100% {
        background-position: -200% 0;
      }
    }
  `]
})
export class SkeletonComponent {
  @Input() type: 'text' | 'circle' | 'card' = 'text';
  @Input() width: string = '100%';
  @Input() height: string = '';
}
