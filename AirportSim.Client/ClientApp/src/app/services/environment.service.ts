import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment.prod';

@Injectable({
  providedIn: 'root'
})
export class EnvironmentService {

  constructor() { }

  getSignalRUrl(){
    return environment.signalRUrl;
  }
}
