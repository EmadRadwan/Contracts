export interface FacilityInventory {
    productId: string;
    partyId: string;
    productName: string;
    partyName: string;
    quantityUomId: string;
    quantityOnHandTotal: number;
    availableToPromiseTotal: number;
    orderedQuantity: number;
    availableToPromiseMinusMinimumStock: number;
    quantityOnHandMinusMinimumStock: number;
    listPrice: number;
}

export interface FacilityInventoryParams {
    orderBy?: string;
    searchTerm?: string;
    pageNumber?: number;
    pageSize?: number;
    filterFlag?: boolean
    filteredProduct?: { productId: string | undefined, productName: string | undefined }
    filteredSupplier?: { fromPartyId: string, fromPartyName: string }

    facilityId?: string;
    productId?: string;
    partyId?: string
    productCategory?: string
    qohMinStockDiff?: number
    atpMinStockdiff?: number
    soldThrough?: Date
    fromDateSellThrough?: Date
    throughDateSellThrough?: Date
    monthsInPastLimit?: number
}

export interface FacilityInventoryItemParams {
    facilityId?: string;
    dateReceivedFrom?: Date
    dateReceivedTo?: Date
    productId?: string
    inventoryItemId?: string

    orderBy?: string;
    searchTerm?: string;
    pageNumber?: number;
    pageSize?: number;
    filterFlag?: boolean
    filteredProduct?: { productId: string | undefined, productName: string | undefined }
}

export interface FacilityInventoryDetailParams {
    facilityId?: string
    effectiveDateFrom?: Date
    effectiveDateTo?: Date
    productId?: string
    inventoryItemId?: string
    orderId?: string
    returnId?: string
    reasonId?: string

    orderBy?: string;
    searchTerm?: string;
    pageNumber?: number;
    pageSize?: number;
    filterFlag?: boolean
    filteredProduct?: { productId: string | undefined, productName: string | undefined }
}