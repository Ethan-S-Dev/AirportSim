import { Component, Input, OnInit } from '@angular/core';
import { faFire, faHeartBroken, faPlane } from '@fortawesome/free-solid-svg-icons';
import { EventDto } from 'src/app/dtos/event-dto';
import { Airplane } from 'src/app/models/airplane';
import { Station } from 'src/app/models/station';

@Component({
  selector: 'app-station',
  templateUrl: './station.component.html',
  styleUrls: ['./station.component.css']
})
export class StationComponent implements OnInit {

  @Input() model?:Station;

  faAirplane = faPlane;
  faFire = faFire;
  faCracks = faHeartBroken;

  get plane():Airplane|undefined{
    return this.model?.plane;
  }

  get event():EventDto|undefined{
    return this.model?.event;
  }

  constructor() { }

  ngOnInit(): void {
  }

}
