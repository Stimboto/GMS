import { Injectable, inject } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { environment } from '../../../environments/environment';
import { AuthService } from './auth.service';
import { Subject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  private hubConnection: signalR.HubConnection | undefined;
  private authService = inject(AuthService);
  
  public notificationReceived = new Subject<{ title: string, message: string }>();

  public startConnection = () => {
    const token = this.authService.getToken();
    if (!token) return;

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(environment.apiUrl.replace('/api', '') + '/hubs/notification', {
        accessTokenFactory: () => token
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection
      .start()
      .then(() => console.log('SignalR Connection started'))
      .catch(err => console.log('Error while starting SignalR connection: ' + err));
      
    this.addNotificationListener();
  }

  private addNotificationListener = () => {
    if (this.hubConnection) {
      this.hubConnection.on('ReceiveNotification', (data: { title: string, message: string }) => {
        console.log('Notification received:', data);
        this.notificationReceived.next(data);
      });
    }
  }

  public stopConnection = () => {
    if (this.hubConnection) {
      this.hubConnection.stop();
    }
  }
}
