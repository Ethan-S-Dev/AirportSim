import { Component, OnInit } from '@angular/core';
import { Guid } from 'src/app/models/airplane-dto';
import { StationDto } from 'src/app/models/station-dto';
import { ControlTowerService } from 'src/app/services/control-tower.service';

@Component({
  selector: 'app-airport',
  templateUrl: './airport.component.html',
  styleUrls: ['./airport.component.css']
})
export class AirportComponent implements OnInit {

  stations:StationDto[] = [];

  constructor(private controlTower:ControlTowerService) { }

  ngOnInit(): void {

    this.controlTower.stationsObservable
    .subscribe(stations=>{
      this.stations = stations;
    });
  }

  getPlane(planeId:string|undefined){
    if(!planeId || planeId === Guid.empty)
      return;

    let plane = this.controlTower.airplanesObservable.value.find(p=>p.id == planeId);
    return plane;
  }

}
