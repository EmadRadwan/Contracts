export interface ProductFacility {
    productId: string;
    facilityId: string;
    facilityName: string;
    minimumStock: number;
    reorderQuantity: number;
    daysToShip?: any;
    replenishMethodEnumId?: any;
    lastInventoryCount: number;
    requirementMethodEnumId?: any;
}