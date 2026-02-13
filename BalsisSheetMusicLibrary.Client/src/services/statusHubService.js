import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';

class StatusHubService {
  static HUB_URL = '/api/statusHub';
  // Have not tested the performance of this
  // May need to implement init(), if building is too slow as this will run on import
  static connection = new HubConnectionBuilder()
    .withUrl(StatusHubService.HUB_URL)
    .configureLogging(LogLevel.Information)
    .build();
  static listeners = [];

  static async start() {
    if (StatusHubService.connection.state !== 'Disconnected') return;
    await StatusHubService.connection.start();
  }

  static async onStatus(callback) {
    if (StatusHubService.connection.state !== 'Connected') {
      await StatusHubService.start();
    }
    
    StatusHubService.connection.on('status', callback);
    StatusHubService.listeners.push(callback);
  }

  static offStatus(callback) {
    StatusHubService.connection.off('status', callback);
    StatusHubService.listeners = StatusHubService.listeners.filter(l => l !== callback);
  }

  static async stop() {
    StatusHubService.listeners.forEach(cb => StatusHubService.connection.off('status', cb));
    StatusHubService.listeners = [];
    await StatusHubService.connection.stop();
  }
}

export default StatusHubService;
