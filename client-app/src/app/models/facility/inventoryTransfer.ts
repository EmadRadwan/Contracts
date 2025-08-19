export interface InventoryTransfer {
    inventoryTransferId?: string;
    statusId?: string;
    statusDescription?: string;
    inventoryItemId?: string;
    productId?: string;
    productName?: string;
    facilityId?: string;
    facilityName?: string;
    locationSeqId?: string;
    containerId?: string;
    facilityIdTo?: string;
    facilityToName?: string;
    locationSeqIdTo?: string;
    containerIdTo?: string;
    itemIssuanceId?: string;
    atpQoh?: string;
    sendDate?: Date;
    receiveDate?: Date | null | undefined;
    comments?: string;
    inventoryComments?: string;
    transferQuantity?: number;
    quantityOnHandTotal?: number;
    availableToPromiseTotal?: number;
}
