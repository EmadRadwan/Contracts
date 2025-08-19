export interface BOMProductComponent {
    productId: string;
    productName: string;
    productDescription: string;
    productIdTo: string;
    productNameTo: string;
    productDescriptionTo: string;
    productAssocTypeId: string;
    fromDate: Date;
    thruDate?: Date | null;
    sequenceNum?: number | null;
    reason?: string | null;
    quantity?: number | null;
    scrapFactor?: number | null;
    instruction?: string | null;
    routingWorkEffortId?: string | null;
    estimateCalcMethod?: string | null;
    recurrenceInfoId?: string | null;
}
