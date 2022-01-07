import { Component, Input, OnInit } from '@angular/core';
import { faFire, faHeartBroken, faPlane } from '@fortawesome/free-solid-svg-icons';
import { AirplaneDto } from 'src/app/models/airplane-dto';
import { EventDto } from 'src/app/models/event-dto';
import { StationDto } from 'src/app/models/station-dto';
import { ControlTowerService } from 'src/app/services/control-tower.service';

@Component({
  selector: 'app-station',
  templateUrl: './station.component.html',
  styleUrls: ['./station.component.css']
})
export class StationComponent implements OnInit {

  @Input() data?:StationDto;

  faAirplane = faPlane;
  faFire = faFire;
  faCracks = faHeartBroken;

  planes:AirplaneDto[] = [];
  events:EventDto[] = [];

  get plane():AirplaneDto|undefined{
    let plane = this.controlTower.airplanesObservable.value.find(p=>p.id === this.data?.currentPlaneId);
    return plane;
  }

  get event():EventDto|undefined{
    let event = this.controlTower.eventsObservable.value.find(e=>e.id === this.data?.eventId);
    return event;
  }

  constructor(private controlTower:ControlTowerService) { }

  ngOnInit(): void {
    this.controlTower.airplanesObservable.subscribe(ps=>this.planes=ps);
    this.controlTower.eventsObservable.subscribe(es=>this.events = es);
  }

}
