export interface CustomerRequestItem {
    custRequestId: string;
    custRequestItemSeqId: string;
    custRequestResolutionId?: any;
    statusId: string;
    priority?: any;
    sequenceNum?: any;
    requiredByDate?: any;
    productId: any;
    productName?: string;
    isProductDeleted?: boolean;
    quantity: number;
    selectedAmount?: any;
    maximumAmount?: any;
    reservStart?: any;
    reservLength?: any;
    reservPersons?: any;
    configId?: any;
    description?: any;
    story?: any;
    lastUpdatedStamp?: Date;
    createdStamp?: Date;

}