import {makeAutoObservable} from "mobx";
import agent from "../api/agent";

export default class GeoStore {
    geoCountry: any;

    constructor() {
        makeAutoObservable(this);
    }

    loadCountries = async () => {
        try {
            this.geoCountry = await agent.Geos.ListCountry();
        } catch (error) {
            console.log(error);
        }
    }
}
