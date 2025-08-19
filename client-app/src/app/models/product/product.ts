export interface Product {
    productId: string;
    goodIdentificationId: string;
    productTypeId: string;
    productTypeDescription: string;
    primaryProductCategoryId: string;
    primaryProductCategoryDescription: string;
    internalName: string | null;
    brandName: string | null;
    comments: string | null;
    productName: string;
    facilityId: string | null;
    introductionDate: Date | null;
    releaseDate: string | null;
    supportDiscontinuationDate: Date | null;
    salesDiscontinuationDate: string | null;
    salesDiscWhenNotAvail: string | null;
    description: string | null;
    longDescription: string | null;
    priceDetailText: string | null;
    smallImageUrl: string | null;
    mediumImageUrl: string | null;
    largeImageUrl: string | null;
    detailImageUrl: string | null;
    originalImageUrl: string | null;
    detailScreen: string | null;
    inventoryMessage: string | null;
    inventoryItemTypeId: string | null;
    requireInventory: string | null;
    quantityUomId: string | null;
    quantityIncluded: number | null;
    piecesIncluded: number | null;
    requireAmount: string | null;
    fixedAmount: number | null;
    amountUomTypeId: string | null;
    weightUomId: string | null;
    shippingWeight: number | null;
    productWeight: number | null;
    heightUomId: string | null;
    productHeight: number | null;
    shippingHeight: number | null;
    widthUomId: string | null;
    productWidth: number | null;
    shippingWidth: number | null;
    depthUomId: string | null;
    productDepth: number | null;
    shippingDepth: number | null;
    diameterUomId: string | null;
    productDiameter: number | null;
    productRating: number | null;
    ratingTypeEnum: string;
    returnable: string | null;
    taxable: string | null;
    chargeShipping: string | null;
    autoCreateKeywords: string | null;
    includeInPromotions: string | null;
    isVirtual: string | null;
    isVariant: string | null;
    virtualVariantMethodEnum: string | null;
    originGeoId: string | null;
    requirementMethodEnumId: string | null;
    billOfMaterialLevel: number | null;
    reservMaxPersons: number | null;
    reserv2ndPPPerc: number | null;
    reservNthPPPerc: number | null;
    configId: string | null;
    createdDate: string | null;
    createdByUserLogin: string | null;
    lastModifiedDate: string | null;
    lastModifiedByUserLogin: string | null;
    inShippingBox: string | null;
    defaultShipmentBoxTypeId: string | null;
    lotIdFilledIn: string | null;
    orderDecimalQuantity: string | null;
    lastUpdatedStamp: string | null;
    createdStamp: string | null;
}

export interface ProductLov {
    productId: string;
    productName: string;
    facilityName: string;
    inventoryItem: string;
    quantityOnHandTotal: number;
    availableToPromiseTotal: number;
    priceWithTax: number;
    lastPrice: number;
}

export interface ServiceLov {
    productId: string;
    productName: string;
}

export interface ProductParams {
    orderBy: string;
    searchTerm?: string;
    productTypes?: string[];
    productCategory?: string;
    pageNumber?: number;
    pageSize?: number;
    productName?: string;
}

export interface ServiceProductPriceParams {
    productId: string;
    vehicleId?: string;
}
