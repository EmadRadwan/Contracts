export interface SupplierProduct {
    productId: string;
    partyId: string;
    partyName: string;
    availableFromDate: Date;
    availableThruDate?: any;
    supplierPrefOrderId?: any;
    supplierRatingTypeId?: any;
    standardLeadTimeDays?: any;
    minimumOrderQuantity: number;
    orderQtyIncrements?: any;
    unitsIncluded?: any;
    quantityUomId: string;
    quantityUomDescription: string;
    agreementId?: any;
    agreementItemSeqId?: any;
    lastPrice: number;
    shippingPrice?: any;
    currencyUomId: string;
    currencyUomDescription: string;
    supplierProductName?: any;
    supplierProductId?: any;
    canDropShip?: any;
    comments?: any;
    lastUpdatedStamp: Date;
    createdStamp: Date;
}
