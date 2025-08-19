export interface InventoryItemDetail {
    inventoryItemId: string;
    facilityId: string;
    orderId: string;
    orderItemSeqId: any;
    productName: string;
    quantityUomId: string;
    quantityOnHandTotal: number;
    availableToPromiseTotal: number;
    quantityOnHandDiff: number;
    availableToPromiseDiff: number;
    effectiveDate: Date;

}

export interface InventoryItemDetailParams {
    inventoryItemId?: string;
    searchTerm?: string;
    facilityId?: string;
    productId?: string;
    pageNumber: number;
    pageSize: number;
}