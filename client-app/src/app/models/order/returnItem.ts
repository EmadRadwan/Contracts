export interface ReturnItem {
    returnId: string;
    returnItemSeqId: string;
    returnReasonId?: string;
    returnTypeId?: string;
    returnItemTypeId?: string;
    productId?: string;
    description?: string;
    orderId?: string;
    orderItemSeqId?: string;
    statusId?: string;
    expectedItemStatus?: string;
    orderQuantity?: any;
    returnQuantity?: any;
    receivedQuantity?: number;
    returnPrice?: number;
    returnItemResponseId?: string;
    isProductToBeReturned?: boolean;
    inEdit?: boolean | string;
    lastUpdatedStamp?: Date;
    lastUpdatedTxStamp?: Date;
    createdStamp?: Date;
    createdTxStamp?: Date;
}