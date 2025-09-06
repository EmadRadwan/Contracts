export interface CertificateItem {
    workEffortId?: string;
    workEffortParentId?: string;
    description?: string;
    quantity?: number;
    uomId?: number; // Aligned with provided interface
    uomName?: string;
    unitPrice?: number;
    totalAmount?: number;
    discount?: number; // Always a value (not percentage)
    insurance?: number; // Always a value (not percentage)
    completionPercentage?: number;
    productId?: string;
    productName?: string;
    facilityId?: string;
    facilityName?: string;
    isContractorPurchased?: string; // Aligned with provided interface
    notes?: string;
    procurementDate?: string;
    isDeleted?: boolean;
    deductions?: number;
    deserved?: number;
    net?: number;
    achievementPercentage?: number;
}