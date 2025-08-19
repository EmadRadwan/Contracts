export interface ReturnAdjustment {
    returnAdjustmentId: string;
    returnAdjustmentTypeId?: string;
    returnId?: string;
    returnItemSeqId?: string;
    shipGroupSeqId?: string;
    comments?: string;
    description?: string;
    returnTypeId?: string;
    orderAdjustmentId?: string;
    amount?: number;
    productPromoId?: string;
    productPromoRuleId?: string;
    productPromoActionSeqId?: string;
    productFeatureId?: string;
    correspondingProductId?: string;
    taxAuthorityRateSeqId?: string;
    sourceReferenceId?: string;
    sourcePercentage?: number;
    customerReferenceId?: string;
    primaryGeoId?: string;
    secondaryGeoId?: string;
    exemptAmount?: number;
    taxAuthGeoId?: string;
    taxAuthPartyId?: string;
    overrideGlAccountId?: string;
    includeInTax?: string;
    includeInShipping?: string;
    createdDate?: Date;

}
