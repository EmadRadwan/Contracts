export interface ProductPrice {
    id: string;
    productId: string;
    productPriceTypeId: string;
    productPricePurposeId: string | null;
    currencyUomId: string;
    productStoreGroupId: string | null;
    fromDate: Date;
    thruDate: Date | null;
    price: number | null;
    productPriceTypeDescription: string | null;
    currencyUomDescription: string | null;
    termUomId: string | null;
    customPriceCalcService: string | null;
    priceWithoutTax: number | null;
    priceWithTax: number | null;
    taxAmount: number | null;
    taxPercentage: number | null;
    taxAuthPartyId: string | null;
    taxAuthGeoId: string | null;
    taxInPrice: string | null;
    createdByUserLogin: string | null;
    lastModifiedByUserLogin: string | null;
    lastUpdatedStamp: Date | null;
    createdStamp: Date | null;
    rowVersion: any;
}
