import { Component } from '@angular/core';
import { ControlTowerService } from './services/control-tower.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  title = "Ethan's Airport Simulator";

  get isConnected():boolean{
    return this.service.isConnected;
  }    
  isLoading = false;
  
  constructor(private service:ControlTowerService){

  }

  connect(){
    this.isLoading = true;
    this.service.connect()
    .subscribe(m=>this.isLoading = false,e=>this.isLoading = false);
  }

}
