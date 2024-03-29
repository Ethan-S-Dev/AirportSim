import { EventDto } from "./event-dto";

export interface StationDto {
    name:string,
    displayName:string,
    waitTimeInSeconds:number,
    currentPlaneId?:string,
    isEventable:boolean
}
