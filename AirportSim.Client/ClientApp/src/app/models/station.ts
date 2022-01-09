import { EventDto } from "../dtos/event-dto";
import { StationDto } from "../dtos/station-dto";
import { Airplane } from "./airplane";

export class Station{
    public name:string;
    public displayName:string;
    /**
     *
     */
    constructor(station:StationDto|undefined) {
        if(!station)
        {
            this.name = 'noname';
            this.displayName = 'noname';
        }
        else
        {
        this.name = station.name;
        this.displayName = station.displayName;
        }
    }

    public plane?:Airplane;
    public event?:EventDto;

    setPlane(plane:Airplane|undefined){
        this.plane = plane;
    }

    setEvent(event:EventDto|undefined){
        this.event = event;
    }
} 