import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { EnvironmentService } from './environment.service';
import { Observable,from, BehaviorSubject} from 'rxjs';
import { AirplaneDto } from '../models/airplane-dto';
import { AirportDto } from '../models/airport-dto';
import { EventDto } from '../models/event-dto';
import { StationDto } from '../models/station-dto';

@Injectable({
  providedIn: 'root'
})
export class ControlTowerService {

  airplanesObservable:BehaviorSubject<AirplaneDto[]> = new BehaviorSubject<AirplaneDto[]>([]);
  stationsObservable:BehaviorSubject<StationDto[]>= new BehaviorSubject<StationDto[]>([]);
  eventsObservable:BehaviorSubject<EventDto[]>= new BehaviorSubject<EventDto[]>([]);


  private get airplanes(){
    return this.airplanesObservable.value;
  }
  private get stations(){
    return this.stationsObservable.value;
  }
  private get events(){
    return this.eventsObservable.value;
  }
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
    this.connection?.on('InitializeAirport',(airport:AirportDto)=>
    {
      this.airplanesObservable.next(airport.airplanes);
      this.stationsObservable.next(airport.stations);
      this.eventsObservable.next(airport.events);
    });

    this.connection?.on('AddAirplane',(p)=>this.addPlane(p));
    this.connection?.on('RemoveAirplane',(p)=>this.removePlane(p));
    this.connection?.on('StartLanding',(p)=>this.startLanding(p));
    this.connection?.on('MoveStation',(p)=>this.moveStation(p));

    this.connection?.on('AddEvent',(event:EventDto)=>{
      
    });
    this.connection?.on('RemoveEvent',(event:EventDto)=>{
      
    });
    this.connection?.on('StartEvent',(event:EventDto)=>{
      
    });
  }

  private addPlane(plane:AirplaneDto){
    console.log(`Airplane with id: ${plane.id} entered`);
    this.airplanesObservable.next([...this.airplanes,plane]);
  }

  private removePlane(plane:AirplaneDto){
    console.log(`Airplane with id: ${plane.id} removed`);
    let oldStation = this.stations.find(s=>s.currentPlaneId === plane.id);
    if(!oldStation)
      return;
      
    oldStation.currentPlaneId = undefined;
    this.airplanesObservable.next(this.airplanes.filter(p=>p.id !== plane.id));
    this.stationsObservable.next(this.stations);
  }

  private startLanding(plane:AirplaneDto){
    let newList = this.airplanes.filter(p=>p.id !== plane.id);
    let station = this.stations.find(s=>s.name == plane.currentStationName);

    if(!station)
      return;

    console.log(`Airplane with id: ${plane.id} start on station: ${station.name}`);
    station.currentPlaneId = plane.id;
    this.airplanesObservable.next([...newList,plane]);
    this.stationsObservable.next(this.stations);
  }

  private moveStation(plane:AirplaneDto){
    let oldPlane = this.airplanes.find(p=>p.id === plane.id);
    let newList = this.airplanes.filter(p=>p.id !== plane.id);   
    let oldStation = this.stations.find(s=>s.name === oldPlane?.currentStationName);
    let newStation =  this.stations.find(s=>s.name === plane.currentStationName);

    if(!newStation || !oldStation)
      return;

    console.log(`Airplane with id: ${plane.id} moved form: ${oldStation.name} to: ${newStation.name}`);
    oldStation.currentPlaneId = undefined;
    newStation.currentPlaneId = plane.id;
    this.airplanesObservable.next([...newList,plane]);
    this.stationsObservable.next(this.stations);
  }
}
