import { HttpClient, HttpHandler, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { EnvironmentService } from './environment.service';

@Injectable({
  providedIn: 'root'
})
export class ControlPanelService {

  private baseUrl:string;
  private options:any;

  constructor(private http:HttpClient,private env:EnvironmentService) { 
    this.baseUrl = this.env.getControlPanelUrl();
    this.options = {headers:new HttpHeaders("Access-Control-Allow-Origin")}; // 
  }

  sendStop(){
    this.http.get(`${this.baseUrl}/stop`,this.options).subscribe(()=>console.log("Stopped successfully!"));
  }

  sendStart(){
    this.http.get(`${this.baseUrl}/start`,this.options).subscribe(()=>console.log("Started successfully!"));
  }

  sendLand(){
    this.http.get<any>(`${this.baseUrl}/land`,this.options).subscribe((m)=>console.log(m));
  }

  sendDeparture(){
    this.http.get<any>(`${this.baseUrl}/departure`,this.options).subscribe((m)=>console.log(m));
  }

  sendFire(){
    this.http.get<any>(`${this.baseUrl}/event/fire`,this.options).subscribe((m)=>console.log(m));
  }

  sendCracks(){
    this.http.get<any>(`${this.baseUrl}/event/cracks`,this.options).subscribe((m)=>console.log(m));
  }
}
