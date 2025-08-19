export interface OrderAdjustment {
    orderAdjustmentId: string;
    orderAdjustmentTypeId?: string;
    orderAdjustmentTypeDescription?: string;
    orderId?: string;
    orderItemSeqId?: string | null;
    isManual?: string;
    comments?: any;
    description?: any;
    amount?: number;
    correspondingProductId?: string;
    correspondingProductName?: string;
    sourcePercentage?: any;
    isAdjustmentDeleted?: boolean;
}