import { Component, EventEmitter, Output, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-file-upload',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatButtonModule],
  template: `
    <div 
      class="drop-zone" 
      [class.dragover]="isDragOver"
      (dragover)="onDragOver($event)"
      (dragleave)="onDragLeave($event)"
      (drop)="onDrop($event)"
      (click)="fileInput.click()">
      
      <mat-icon class="upload-icon">cloud_upload</mat-icon>
      <h3>Drag files here or Browse Files</h3>
      <p>Supported: PNG, JPG, PDF, DOCX</p>
      
      <input 
        type="file" 
        #fileInput 
        hidden 
        [multiple]="multiple" 
        (change)="onFileSelected($event)" 
        accept=".png,.jpg,.jpeg,.pdf,.docx,.doc">
    </div>

    <div class="selected-files" *ngIf="files.length > 0">
      <h4>Selected Files:</h4>
      <ul>
        <li *ngFor="let file of files; let i = index">
          <mat-icon>insert_drive_file</mat-icon>
          <span>{{ file.name }}</span>
          <button mat-icon-button color="warn" (click)="removeFile(i); $event.stopPropagation()">
            <mat-icon>close</mat-icon>
          </button>
        </li>
      </ul>
    </div>
  `,
  styles: [`
    .drop-zone {
      border: 2px dashed var(--border-color);
      border-radius: 12px;
      padding: 3rem 2rem;
      text-align: center;
      background: var(--bg-paper);
      cursor: pointer;
      transition: all 0.3s ease;
      margin-bottom: 1rem;
    }
    .drop-zone:hover, .drop-zone.dragover {
      border-color: var(--primary-color);
      background: rgba(37, 99, 235, 0.05);
    }
    .upload-icon {
      font-size: 48px;
      width: 48px;
      height: 48px;
      color: var(--primary-color);
      margin-bottom: 1rem;
    }
    h3 {
      font-size: 1.1rem;
      color: var(--text-primary);
      margin-bottom: 0.5rem;
    }
    p {
      color: var(--text-secondary);
      font-size: 0.9rem;
    }
    .selected-files h4 {
      margin-bottom: 0.5rem;
      color: var(--text-primary);
    }
    .selected-files ul {
      list-style: none;
      padding: 0;
    }
    .selected-files li {
      display: flex;
      align-items: center;
      gap: 12px;
      padding: 8px 12px;
      background: var(--bg-paper);
      border: 1px solid var(--border-color);
      border-radius: 8px;
      margin-bottom: 8px;
    }
    .selected-files li span {
      flex: 1;
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
      color: var(--text-primary);
    }
  `]
})
export class FileUploadComponent {
  @Input() multiple = true;
  @Output() filesChanged = new EventEmitter<File[]>();

  isDragOver = false;
  files: File[] = [];

  onDragOver(event: DragEvent) {
    event.preventDefault();
    this.isDragOver = true;
  }

  onDragLeave(event: DragEvent) {
    event.preventDefault();
    this.isDragOver = false;
  }

  onDrop(event: DragEvent) {
    event.preventDefault();
    this.isDragOver = false;
    
    if (event.dataTransfer?.files) {
      this.handleFiles(Array.from(event.dataTransfer.files));
    }
  }

  onFileSelected(event: any) {
    if (event.target.files) {
      this.handleFiles(Array.from(event.target.files));
    }
  }

  handleFiles(newFiles: File[]) {
    // Filter valid extensions
    const validExtensions = ['png', 'jpg', 'jpeg', 'pdf', 'docx', 'doc'];
    const validFiles = newFiles.filter(file => {
      const ext = file.name.split('.').pop()?.toLowerCase();
      return ext && validExtensions.includes(ext);
    });

    if (this.multiple) {
      this.files = [...this.files, ...validFiles];
    } else {
      this.files = validFiles.length > 0 ? [validFiles[0]] : [];
    }
    this.filesChanged.emit(this.files);
  }

  removeFile(index: number) {
    this.files.splice(index, 1);
    this.filesChanged.emit(this.files);
  }
}
