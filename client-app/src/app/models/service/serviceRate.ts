export interface ServiceRate {
    serviceRateId?: string;
    makeId: string;
    makeDescription?: string;
    modelId: string;
    modelDescription?: string;
    productStoreId?: string;
    productStoreName?: string;
    rate: number;
    fromDate: Date;
    thruDate?: Date;
}

