export interface ProductAssociation {
    productId: string
    productIdTo: string
    productAssocTypeId: string
    fromDate: Date
    thruDate?: Date
    sequenceNum?: number
    reason?: string
    quantity: number
    scrapFactor?: number
    instruction?: string
    routingWorkEffortId?: string
    estimateCalcMethod?: string
    recurrenceInfoId?: string
}