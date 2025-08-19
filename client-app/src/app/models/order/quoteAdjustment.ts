export interface QuoteAdjustment {
    quoteAdjustmentId: string;
    quoteAdjustmentTypeId?: string;
    quoteAdjustmentTypeDescription?: string;
    quoteId?: string;
    quoteItemSeqId?: string;
    comments?: any;
    description?: any;
    isManual?: string;
    amount?: number;
    correspondingProductId?: any;
    correspondingProductName?: any;
    sourcePercentage?: any;
    isAdjustmentDeleted?: boolean;

}