import { AirplaneDto } from "./airplane-dto";
import { EventDto } from "./event-dto";
import { StationDto } from "./station-dto";

export interface AirportDto {
    airplanes:AirplaneDto[],
    events:EventDto[],
    stations:StationDto[]
}
