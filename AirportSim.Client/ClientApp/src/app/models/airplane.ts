import { AirplaneDto } from "../dtos/airplane-dto";

export class Airplane{
    public id:string;
    public color:string;
    public type:string;
    /**
     *
     */
    constructor(planeDto:AirplaneDto) {
        this.id = planeDto.id;
        this.type = planeDto.type;
        this.color = this.getRandomColor();
    }

    private getRandomColor():string{
        var randomColor = Math.floor(Math.random()*16777215).toString(16);
        return `#${randomColor}`;
      }
}