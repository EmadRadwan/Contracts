export interface InventoryItem {
    inventoryItemId: string;
    facilityId: string;
    facilityName: string;
    productId: any;
    productName: string;
    quantityUomId: string;
    quantityOnHandTotal: number;
    availableToPromiseTotal: number;
    orderedQuantity: number;
    datetimeReceived: Date;
    expireDate: Date;
    datetimeManufactured: Date;
    statusId: string;
    comments: string;
}

export interface InventoryItemParams {
    searchTerm?: string;
    facilityId: string;
    productId: string;
    pageNumber: number;
    pageSize: number;
}