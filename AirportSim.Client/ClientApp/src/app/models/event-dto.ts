export interface EventDto {
    id:string,
    eventType:string,
    eventTimeInSeconds:number,
    receivedAt:Date,
    isStarted:boolean,
    stationName:string
}
