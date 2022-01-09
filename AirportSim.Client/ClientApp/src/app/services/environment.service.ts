import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root'
})
export class EnvironmentService {

  constructor() { }

  getSignalRUrl(){
    return environment.signalRUrl;
  }

  getControlPanelUrl(){
    return environment.controlPanelUrl;
  }
}
