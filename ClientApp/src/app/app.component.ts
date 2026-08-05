import { Component, inject, OnInit } from '@angular/core';
import { RouterOutlet, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from './core/services/auth.service';
import { SignalRService } from './core/services/signalr.service';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterModule, CommonModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent implements OnInit {
  title = 'GMS Frontend';
  authService = inject(AuthService);
  signalRService = inject(SignalRService);
  snackBar = inject(MatSnackBar);

  ngOnInit() {
    this.authService.currentUser$.subscribe(user => {
      if (user) {
        this.signalRService.startConnection();
        this.signalRService.notificationReceived.subscribe((notif) => {
          this.snackBar.open(`${notif.title}: ${notif.message}`, 'Close', {
            duration: 5000,
            horizontalPosition: 'right',
            verticalPosition: 'top',
          });
        });
      } else {
        this.signalRService.stopConnection();
      }
    });
  }
}
