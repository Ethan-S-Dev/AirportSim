import { HttpClient } from '@angular/common/http';
import { EventEmitter, Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { EnvironmentService } from './environment.service';
import { Observable,from, BehaviorSubject} from 'rxjs';
import { AirplaneDto } from '../dtos/airplane-dto';
import { AirportDto } from '../dtos/airport-dto';
import { EventDto } from '../dtos/event-dto';
import { StationDto } from '../dtos/station-dto';
import { Station } from '../models/station';

@Injectable({
  providedIn: 'root'
})
export class ControlTowerService {

  onAirplaneAdded:EventEmitter<AirplaneDto> = new EventEmitter<AirplaneDto>();
  onAirplaneStarted:EventEmitter<AirplaneDto> = new EventEmitter<AirplaneDto>();
  onAirplaneMoved:EventEmitter<AirplaneDto> = new EventEmitter<AirplaneDto>();
  onAirplaneRemoved:EventEmitter<AirplaneDto> = new EventEmitter<AirplaneDto>();

  onEventAdded:EventEmitter<EventDto> = new EventEmitter<EventDto>();
  onEventStarted:EventEmitter<EventDto> = new EventEmitter<EventDto>();
  onEventRemoved:EventEmitter<EventDto> = new EventEmitter<EventDto>();

  onInitialize:EventEmitter<AirportDto> = new EventEmitter<AirportDto>();

  private connection?:signalR.HubConnection;
  get isConnected():boolean{
    return this.connection ? this.connection?.state == signalR.HubConnectionState.Connected : false;
  }

  constructor(private envService:EnvironmentService) { }

  public connect(): Observable<any>{
    if(this.isConnected)
      return new Observable<any>(subscribe=>{
        subscribe.next(`SignalR connection success! connectionId: ${this.connection?.connectionId} `);
        subscribe.complete();
      });
  
      this.connection = new signalR.HubConnectionBuilder()
        .withUrl(this.envService.getSignalRUrl())
        .build();
  
      this.connection.keepAliveIntervalInMilliseconds = 10000;
      this.connection.serverTimeoutInMilliseconds = 120000;

      this.setSignalRClientMethods();

      return from(this.connection
        .start()
        .then(() => {
          console.log(`SignalR connection success! connectionId: ${this.connection?.connectionId} `);
          return `SignalR connection success! connectionId: ${this.connection?.connectionId} `;
        })
        .catch((error) => {
          console.error(`SignalR connection error: ${error}`);
          return error;
        }));
  }

  public disconnect(){
    this.connection?.stop()
    .then(()=>{
      console.log(`SignalR connection stopped!`);
    });
  }

  private setSignalRClientMethods(){
    this.connection?.on('InitializeAirport',(airport:AirportDto)=>this.onInitialize.emit(airport));

    this.connection?.on('AddAirplane',(p)=>this.onAirplaneAdded.emit(p));
    this.connection?.on('RemoveAirplane',(p)=>this.onAirplaneRemoved.emit(p));
    this.connection?.on('StartLanding',(p)=>this.onAirplaneStarted.emit(p));
    this.connection?.on('MoveStation',(p)=>this.onAirplaneMoved.emit(p));

    this.connection?.on('AddEvent',(e)=>this.onEventAdded.emit(e));
    this.connection?.on('RemoveEvent',(e)=>this.onEventRemoved.emit(e));
    this.connection?.on('StartEvent',(e)=>this.onEventStarted.emit(e));
  }
}
