import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { EnvironmentService } from './environment.service';
import { Observable,from, BehaviorSubject} from 'rxjs';
import { AirplaneDto } from '../models/airplane-dto';
import { AirportDto } from '../models/airport-dto';
import { EventDto } from '../models/event-dto';

@Injectable({
  providedIn: 'root'
})
export class ControlTowerService {

  airplaneAdded:BehaviorSubject<AirplaneDto|undefined> = new BehaviorSubject<AirplaneDto|undefined>(undefined);
  airplaneRemoved:BehaviorSubject<AirplaneDto|undefined> = new BehaviorSubject<AirplaneDto|undefined>(undefined);
  airplaneStartLanding:BehaviorSubject<AirplaneDto|undefined> = new BehaviorSubject<AirplaneDto|undefined>(undefined);
  airplaneMovedStation:BehaviorSubject<AirplaneDto|undefined> = new BehaviorSubject<AirplaneDto|undefined>(undefined);

  eventAdded:BehaviorSubject<EventDto|undefined> = new BehaviorSubject<EventDto|undefined>(undefined);
  eventRemoved:BehaviorSubject<EventDto|undefined> = new BehaviorSubject<EventDto|undefined>(undefined);
  eventStarted:BehaviorSubject<EventDto|undefined> = new BehaviorSubject<EventDto|undefined>(undefined);

  airportInitialized:BehaviorSubject<AirportDto|undefined> = new BehaviorSubject<AirportDto|undefined>(undefined);

  private connection?:signalR.HubConnection;
  get isConnected():boolean{
    return this.connection ? this.connection?.state == signalR.HubConnectionState.Connected : false;
  }

  constructor(private envService:EnvironmentService,private http:HttpClient) { }

  public connect(): Observable<any>{
    if(this.isConnected)
      return new Observable<any>(subscribe=>{
        subscribe.next(`SignalR connection success! connectionId: ${this.connection?.connectionId} `);
        subscribe.complete();
      });
  
      this.connection = new signalR.HubConnectionBuilder()
        .withUrl(this.envService.getSignalRUrl())
        .build();
  
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
    this.connection?.on('InitializeAirport',(airport:AirportDto)=>this.airportInitialized.next(airport));

    this.connection?.on('AddAirplane',(plane:AirplaneDto)=>this.airplaneAdded.next(plane));
    this.connection?.on('RemoveAirplane',(plane:AirplaneDto)=>this.airplaneRemoved.next(plane));
    this.connection?.on('StartLanding',(plane:AirplaneDto)=>this.airplaneStartLanding.next(plane));
    this.connection?.on('MoveStation',(plane:AirplaneDto)=>this.airplaneMovedStation.next(plane));

    this.connection?.on('AddEvent',(event:EventDto)=>this.eventAdded.next(event));
    this.connection?.on('RemoveEvent',(event:EventDto)=>this.eventRemoved.next(event));
    this.connection?.on('StartEvent',(event:EventDto)=>this.eventStarted.next(event));
  }
}
