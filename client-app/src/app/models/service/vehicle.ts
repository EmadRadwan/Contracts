export interface Vehicle {
    vehicleId: any;
    chassisNumber: string;
    vin?: string;
    year?: number;
    plateNumber: string;

    // Rest of your properties

    fromPartyId: any;
    fromPartyName: string;
    makeId: string;
    makeDescription: string;
    modelId: string;
    modelDescription: string;
    vehicleTypeId: string;
    vehicleTypeDescription: string;
    transmissionTypeId: string;
    transmissionTypeDescription: string;
    exteriorColorId: string;
    exteriorColorDescription: string;
    interiorColorId: string;
    interiorColorDescription: string;

    serviceDate: any;
    mileage: number;
    nextServiceDate: Date;
}

export interface VehicleParams {
    orderBy: string;
    searchTerm?: string;
    customerPhone?: string;
    pageNumber?: number;
    pageSize?: number;
    chassisNumber?: string;
    vin?: string;
    plateNumber?: string;
    ownerPartyId?: string;
    makeId?: string;
    modelId?: string;
    vehicleTypes?: string[];
}


export interface Make {
    makeId: string
    makeDescription: string
}

export interface Model {
    makeId: string
    modelId: string
    modelDescription: string
}

export interface VehicleLov {
    vehicleId: string;
    chassisNumber: string;
    makeDescription: string;
    modelDescription: string;
    plateNumber: string;
    fromPartyId: any;
    fromPartyName: string;
    serviceDate: string;
    nextServiceDate: Date;
}
