export class Guid{
    static _empty = "00000000-0000-0000-0000-000000000000";
    public static get empty():string{
        return this._empty;
    }
}

export interface AirplaneDto {
    id:string,
    type:string,
    isOutside:boolean,
    objective:string,
    currentStationName?:string,
    enteredAt:Date,
    color:string
}
