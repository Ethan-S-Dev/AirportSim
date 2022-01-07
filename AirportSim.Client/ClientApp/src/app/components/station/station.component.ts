import { Component, Input, OnInit } from '@angular/core';
import { AirplaneDto } from 'src/app/models/airplane-dto';
import { StationDto } from 'src/app/models/station-dto';
import { ControlTowerService } from 'src/app/services/control-tower.service';

@Component({
  selector: 'app-station',
  templateUrl: './station.component.html',
  styleUrls: ['./station.component.css']
})
export class StationComponent implements OnInit {

  @Input() data?:StationDto;

  planes:AirplaneDto[] = [];

  get plane():AirplaneDto|undefined{
    let plane = this.controlTower.airplanesObservable.value.find(p=>p.id === this.data?.currentPlaneId);
    return plane;
  }

  constructor(private controlTower:ControlTowerService) { }

  ngOnInit(): void {
    this.controlTower.airplanesObservable.subscribe(ps=>this.planes=ps);
  }

}
