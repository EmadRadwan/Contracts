export interface ProductInventoryItem {
    productId: string;
    productName: string;
    internalName: string;
    facilityId: string;
    inventoryItemTypeId: string;
    inventoryItemId: string;
    productFacilityId: string;
    inventoryComments: string;
    productATP: number | null;
    productQOH: number | null;
}