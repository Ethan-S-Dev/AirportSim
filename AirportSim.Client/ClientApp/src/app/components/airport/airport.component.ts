import { Component, OnInit } from '@angular/core';
import { AirplaneDto, Guid } from 'src/app/dtos/airplane-dto';
import { AirportDto } from 'src/app/dtos/airport-dto';
import { EventDto } from 'src/app/dtos/event-dto';
import { Airplane } from 'src/app/models/airplane';
import { Station } from 'src/app/models/station';
import { ControlPanelService } from 'src/app/services/control-panel.service';
import { ControlTowerService } from 'src/app/services/control-tower.service';

@Component({
  selector: 'app-airport',
  templateUrl: './airport.component.html',
  styleUrls: ['./airport.component.css']
})
export class AirportComponent implements OnInit {

  stations:Station[] = [];
  incomingEvents:EventDto[] = [];
  incomingPlanes:Airplane[] = [];

  constructor(private controlTower:ControlTowerService,private panel:ControlPanelService) { }

  ngOnInit(): void {
    this.controlTower.onInitialize.subscribe(airport=>this.initAirport(airport));

    this.controlTower.onAirplaneStarted.subscribe(p=>this.startPlane(p));
    this.controlTower.onAirplaneMoved.subscribe(p=>this.movePlane(p));
    this.controlTower.onAirplaneAdded.subscribe(p=>this.addPlane(p));
    this.controlTower.onAirplaneRemoved.subscribe(p=>this.removePlane(p));

    this.controlTower.onEventAdded.subscribe(e=>this.addEvent(e));
    this.controlTower.onEventStarted.subscribe(e=>this.startEvent(e));
    this.controlTower.onEventRemoved.subscribe(e=>this.removeEvent(e))
  }

  stop(){
    this.panel.sendStop();
  }

  start(){
    this.panel.sendStart();
  }

  land(){
    this.panel.sendLand();
  }

  departure(){
    this.panel.sendDeparture();
  }

  fire(){
    this.panel.sendFire();
  }

  cracks(){
    this.panel.sendCracks();
  }

  private allPlanes:Airplane[] = [];

  private initAirport(airport: AirportDto) {
    this.stations = airport.stations.map(s => new Station(s));
    airport.airplanes.forEach(p => {
      let plane = new Airplane(p);
      if (p.isOutside) {
        this.allPlanes.push(plane)
        this.incomingPlanes.push(plane);
        return;
      }

      let station = this.stations.find(s => s.name == p.currentStationName);
      station?.setPlane(plane);
    });
    airport.events.forEach(e => {
      if (!e.isStarted) {
        this.incomingEvents.push(e);
        return;
      }

      let station = this.stations.find(s => s.name == e.stationName);
      station?.setEvent(e);
    });
  }

  private addPlane(p:AirplaneDto){
    let plane = new Airplane(p);
    this.allPlanes.push(plane);
    this.incomingPlanes.push(plane);
  }

  private startPlane(p:AirplaneDto){
    let station = this.stations.find(s=>s.name === p.currentStationName);
    this.incomingPlanes = this.incomingPlanes.filter(plane=>plane.id !== p.id);
    let plane = this.allPlanes.find(plane=>plane.id==p.id);
    station?.setPlane(plane);
  }

  private movePlane(p:AirplaneDto){
    let oldStation = this.stations.find(s=>s.plane?.id == p.id);
    oldStation?.setPlane(undefined);

    let station = this.stations.find(s=>s.name === p.currentStationName);
    let plane = this.allPlanes.find(plane=>plane.id==p.id);
    station?.setPlane(plane);
  }

  private removePlane(p:AirplaneDto){
    let oldStation = this.stations.find(s=>s.plane?.id == p.id);
    oldStation?.setPlane(undefined);
  }

  private addEvent(e:EventDto){
    this.incomingEvents.push(e);
  }

  private removeEvent(e: EventDto){
    let station = this.stations.find(s=>s.name === e.stationName);
    station?.setEvent(undefined);
  }
  private startEvent(e: EventDto) {
    this.incomingEvents = this.incomingEvents.filter(event=>event.id !== e.id);

    let station = this.stations.find(s=>s.name === e.stationName);
    station?.setEvent(e);
  }
}
