import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { StatusHistory } from '../../../core/models/models';

@Component({
  selector: 'app-vertical-timeline',
  standalone: true,
  imports: [CommonModule, MatIconModule],
  template: `
    <div class="timeline">
      <div class="timeline-item" *ngFor="let step of steps; let last = last" [class.completed]="step.completed" [class.current]="step.current">
        <div class="timeline-indicator">
          <div class="dot"></div>
          <div class="line" *ngIf="!last"></div>
        </div>
        <div class="timeline-content">
          <div class="timeline-header">
            <h4>{{ step.status }}</h4>
            <span class="timeline-date" *ngIf="step.date">{{ step.date | date:'short' }}</span>
          </div>
          <p class="timeline-remarks" *ngIf="step.remarks">{{ step.remarks }}</p>
          <div class="timeline-attachment" *ngIf="step.attachmentUrl">
             <mat-icon inline="true">attach_file</mat-icon>
             <a [href]="step.attachmentUrl" target="_blank">{{ step.attachmentName || 'View File' }}</a>
          </div>
          <p class="timeline-author" *ngIf="step.author">By: {{ step.author }}</p>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .timeline {
      display: flex;
      flex-direction: column;
      padding: 1rem 0;
    }
    .timeline-item {
      display: flex;
      position: relative;
    }
    .timeline-indicator {
      display: flex;
      flex-direction: column;
      align-items: center;
      margin-right: 16px;
      min-width: 24px;
    }
    .dot {
      width: 16px;
      height: 16px;
      border-radius: 50%;
      background: var(--border-color);
      border: 3px solid white;
      box-shadow: 0 0 0 1px var(--border-color);
      z-index: 2;
    }
    .line {
      width: 2px;
      flex: 1;
      background: var(--border-color);
      margin-top: 4px;
      margin-bottom: 4px;
      min-height: 40px;
    }
    .timeline-content {
      padding-bottom: 24px;
      flex: 1;
    }
    .timeline-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      margin-bottom: 4px;
    }
    .timeline-header h4 {
      margin: 0;
      font-size: 1rem;
      color: var(--text-secondary);
      font-weight: 500;
    }
    .timeline-date {
      font-size: 0.85rem;
      color: var(--text-secondary);
    }
    .timeline-remarks {
      margin: 8px 0 0;
      color: var(--text-primary);
      font-size: 0.95rem;
      line-height: 1.5;
      background: var(--bg-paper);
      padding: 12px;
      border-radius: 8px;
      border: 1px solid var(--border-color);
    }
    .timeline-author {
      margin: 8px 0 0;
      font-size: 0.85rem;
      color: var(--text-secondary);
      font-style: italic;
    }
    .timeline-attachment {
      margin: 8px 0 0;
      display: flex;
      align-items: center;
      gap: 4px;
      font-size: 0.85rem;
    }
    .timeline-attachment a {
      color: var(--primary-color);
      text-decoration: none;
      font-weight: 500;
    }
    .timeline-attachment a:hover {
      text-decoration: underline;
    }

    /* States */
    .timeline-item.completed .dot {
      background: var(--primary-color);
      box-shadow: 0 0 0 1px var(--primary-color);
    }
    .timeline-item.completed .line {
      background: var(--primary-color);
    }
    .timeline-item.completed .timeline-header h4 {
      color: var(--text-primary);
      font-weight: 600;
    }
    
    .timeline-item.current .dot {
      background: white;
      border: 4px solid var(--primary-color);
      box-shadow: 0 0 0 2px rgba(37, 99, 235, 0.2);
    }
    .timeline-item.current .timeline-header h4 {
      color: var(--primary-color);
      font-weight: 700;
    }
  `]
})
export class VerticalTimelineComponent {
  // Steps ordered correctly
  @Input() set histories(val: StatusHistory[] | undefined) {
    if (!val) return;
    
    // Default flow
    const flow = ['Submitted', 'Assigned', 'In Review', 'Resolved', 'Closed'];
    
    this.steps = flow.map(statusName => {
      // Find if this status exists in history
      const historyRecord = val.find(h => h.newStatus === statusName);
      
      return {
        status: statusName,
        completed: !!historyRecord,
        current: false,
        date: historyRecord?.changedAt,
        remarks: historyRecord?.remarks,
        author: historyRecord?.changedByUserName,
        attachmentUrl: historyRecord?.attachmentUrl,
        attachmentName: historyRecord?.attachmentName
      };
    });

    // Mark current
    let lastCompletedIndex = -1;
    for (let i = 0; i < this.steps.length; i++) {
      if (this.steps[i].completed) {
        lastCompletedIndex = i;
      }
    }
    
    if (lastCompletedIndex >= 0) {
      this.steps[lastCompletedIndex].current = true;
    }
  }

  steps: any[] = [];
}
