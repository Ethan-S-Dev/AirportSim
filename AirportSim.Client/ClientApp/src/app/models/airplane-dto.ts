export interface AirplaneDto {
    id:string,
    type:string,
    isOutside:boolean,
    objective:string,
    currentStationName?:string,
    enteredAt:Date
}
