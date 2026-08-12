import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AiService, ChatMessage } from '../../../core/services/ai.service';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-gms-smart-chat',
  standalone: true,
  imports: [CommonModule, FormsModule, MatIconModule, MatButtonModule, MatProgressSpinnerModule],
  template: `
    <div class="chat-page-container">
      <div class="chat-window">
        <div class="chat-header">
          <div class="header-title">
            <div class="bot-avatar">
              <mat-icon>auto_awesome</mat-icon>
            </div>
            <div>
              <h3 class="bot-name">GMS Smart AI</h3>
              <span class="bot-status">Citizen Assistant • Online</span>
            </div>
          </div>
        </div>

        <div class="chat-body" #scrollContainer>
          <div class="welcome-message" *ngIf="messages.length === 0">
            <mat-icon class="welcome-icon">psychology</mat-icon>
            <h4>Welcome to GMS Smart!</h4>
            <p>I am your official AI Citizen Assistant. Ask me anything about departments, status meanings, required documents, or filing grievances.</p>
            
            <div class="chip-container">
              <button *ngFor="let chip of suggestionChips" class="suggestion-chip" (click)="sendPresetMessage(chip)">
                {{ chip }}
              </button>
            </div>
          </div>

          <div *ngFor="let msg of messages" class="message-row" [class.user-row]="msg.sender === 'user'">
            <div class="message-bubble" [class.user-bubble]="msg.sender === 'user'" [class.bot-bubble]="msg.sender === 'bot'">
              <p class="msg-text">{{ msg.text }}</p>
            </div>
          </div>

          <div class="message-row bot-row" *ngIf="loading">
            <div class="message-bubble bot-bubble typing-bubble">
              <mat-spinner diameter="18"></mat-spinner>
              <span>GMS Smart is thinking...</span>
            </div>
          </div>
        </div>

        <div class="chat-footer">
          <input 
            type="text" 
            class="chat-input" 
            placeholder="Ask a question..." 
            [(ngModel)]="userInput" 
            (keyup.enter)="sendMessage()"
            [disabled]="loading"
          />
          <button mat-icon-button color="primary" class="send-btn" (click)="sendMessage()" [disabled]="!userInput.trim() || loading">
            <mat-icon>send</mat-icon>
          </button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .chat-page-container {
      display: flex;
      justify-content: center;
      align-items: center;
      height: 100%;
      padding: 24px;
      background: #f1f5f9;
      font-family: 'Inter', sans-serif;
    }
    .chat-window {
      width: 100%;
      max-width: 800px;
      height: 80vh;
      max-height: 800px;
      background: #ffffff;
      border: 1px solid #e2e8f0;
      border-radius: 16px;
      box-shadow: 0 10px 30px rgba(0, 0, 0, 0.05);
      display: flex;
      flex-direction: column;
      overflow: hidden;
    }
    .chat-header {
      background: linear-gradient(135deg, #4f46e5 0%, #3730a3 100%);
      color: white;
      padding: 16px 24px;
      display: flex;
      align-items: center;
    }
    .header-title {
      display: flex;
      align-items: center;
      gap: 12px;
    }
    .bot-avatar {
      background: rgba(255, 255, 255, 0.2);
      border-radius: 50%;
      width: 40px;
      height: 40px;
      display: flex;
      align-items: center;
      justify-content: center;
      color: #fef08a;
    }
    .bot-avatar mat-icon {
      font-size: 24px;
      width: 24px;
      height: 24px;
    }
    .bot-name {
      margin: 0;
      font-size: 1.1rem;
      font-weight: 700;
    }
    .bot-status {
      font-size: 0.8rem;
      color: #e0e7ff;
    }
    .chat-body {
      flex: 1;
      padding: 24px;
      overflow-y: auto;
      display: flex;
      flex-direction: column;
      gap: 16px;
      background: #f8fafc;
    }
    .welcome-message {
      text-align: center;
      padding: 32px 16px;
      margin: auto;
    }
    .welcome-icon {
      font-size: 48px;
      width: 48px;
      height: 48px;
      color: #6366f1;
      margin-bottom: 12px;
    }
    .welcome-message h4 {
      margin: 0 0 8px 0;
      font-size: 1.5rem;
      color: #1e293b;
    }
    .welcome-message p {
      font-size: 0.95rem;
      color: #64748b;
      line-height: 1.5;
      margin-bottom: 24px;
      max-width: 500px;
      margin-left: auto;
      margin-right: auto;
    }
    .chip-container {
      display: flex;
      flex-wrap: wrap;
      gap: 10px;
      justify-content: center;
    }
    .suggestion-chip {
      background: #ffffff;
      border: 1px solid #cbd5e1;
      border-radius: 20px;
      padding: 8px 16px;
      font-size: 0.85rem;
      color: #334155;
      cursor: pointer;
      transition: all 0.2s ease;
    }
    .suggestion-chip:hover {
      background: #f8fafc;
      border-color: #6366f1;
      color: #4f46e5;
    }
    .message-row {
      display: flex;
    }
    .user-row {
      justify-content: flex-end;
    }
    .message-bubble {
      max-width: 75%;
      padding: 12px 16px;
      border-radius: 12px;
      font-size: 0.95rem;
      line-height: 1.5;
    }
    .user-bubble {
      background: #4f46e5;
      color: #ffffff;
      border-bottom-right-radius: 2px;
    }
    .user-bubble .msg-text {
      color: #ffffff;
    }
    .bot-bubble {
      background: #ffffff;
      color: #1e293b;
      border: 1px solid #e2e8f0;
      border-bottom-left-radius: 2px;
    }
    .typing-bubble {
      display: flex;
      align-items: center;
      gap: 10px;
      color: #64748b;
    }
    .msg-text {
      margin: 0;
      white-space: pre-wrap;
      word-break: break-word;
    }
    .chat-footer {
      padding: 16px 20px;
      background: #ffffff;
      border-top: 1px solid #e2e8f0;
      display: flex;
      align-items: center;
      gap: 12px;
    }
    .chat-input {
      flex: 1;
      border: 1px solid #cbd5e1;
      border-radius: 24px;
      padding: 12px 20px;
      font-size: 0.95rem;
      outline: none;
      transition: border-color 0.2s ease;
    }
    .chat-input:focus {
      border-color: #6366f1;
    }
    .send-btn {
      color: #4f46e5;
      transform: scale(1.1);
    }
  `]
})
export class GmsSmartChatComponent {
  private aiService = inject(AiService);

  loading = false;
  userInput = '';
  messages: ChatMessage[] = [];

  suggestionChips = [
    'Which department should I choose?',
    'How do I file a grievance?',
    'What does "Resolved" mean?',
    'How can I track my grievance?'
  ];

  sendPresetMessage(text: string) {
    this.userInput = text;
    this.sendMessage();
  }

  sendMessage() {
    if (!this.userInput.trim() || this.loading) return;

    const userText = this.userInput.trim();
    this.userInput = '';

    this.messages.push({ sender: 'user', text: userText });
    this.loading = true;

    this.aiService.chat({ message: userText, history: this.messages }).subscribe({
      next: (res) => {
        this.loading = false;
        this.messages.push({ sender: 'bot', text: res.reply });
      },
      error: () => {
        this.loading = false;
        this.messages.push({ 
          sender: 'bot', 
          text: 'I am currently undergoing maintenance. Please try again shortly or contact our support team.' 
        });
      }
    });
  }
}
