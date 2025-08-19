export interface ReceiveInventoryItems {
    orderItems?: ReceiveInventoryItems[]; 
}

export interface ReceiveInventoryItem {
    orderId?: string;
    orderItemSeqId?: string;
    productId?: string;
    quantityAccepted?: number;
    quantityRejected?: number;
    rejectionReasonId?: string;
    facilityId?: string;
    unitPrice?: number;
}
