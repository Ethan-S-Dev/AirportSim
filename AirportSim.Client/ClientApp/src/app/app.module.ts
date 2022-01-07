import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import {HttpClientModule} from '@angular/common/http';

import { AppComponent } from './app.component';
import { SpinnerComponent } from './components/spinner/spinner.component';
import { AirportComponent } from './components/airport/airport.component';
import { StationComponent } from './components/station/station.component';

@NgModule({
  declarations: [
    AppComponent,
    SpinnerComponent,
    AirportComponent,
    StationComponent
  ],
  imports: [
    BrowserModule,
    HttpClientModule
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }