export interface Facility {
    facilityId: string;
    facilityTypeId: string;
    facilityTypeDescription: string;
    parentFacilityId: string;
    ownerPartyId: string;
    defaultInventoryItemTypeId: string;
    facilityName: string;
    primaryFacilityGroupId: string;
    facilitySizeUomId: string;
    productStoreId: string;
    description: string;
    defaultDimensionUomId: string;
    defaultWeightUomId: string;
    geoPointId: string;
    squareFootage: number | null;
    facilitySize: number | null;
    defaultDaysToShip: number | null;
    openedDate: string | null;
    closedDate: string | null;
    facilityLevel: number | null;
    lastUpdatedStamp: string | null;
    createdStamp: string | null;
}