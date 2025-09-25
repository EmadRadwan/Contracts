export interface CertificateItem {
    workEffortId?: string;
    workEffortParentId?: string;
    description?: string;
    deductionDescription?: string;
    quantity?: number;
    uomId?: string;
    uomName?: string;
    materialPrice?: number; // Used for WORKMANSHIP_CONTRACTING_CERTIFICATE
    laborPrice?: number;    // Used for WORKMANSHIP_CONTRACTING_CERTIFICATE
    unitPrice?: number;     // Sum of materialPrice and laborPrice for WORKMANSHIP_CONTRACTING_CERTIFICATE, or standalone for other types
    totalAmount?: number;
    discount?: number; // Always a value (not percentage)
    insurance?: number; // Always a value (not percentage)
    insuranceMode?: "value" | "percentage";
    additionalInsuranceMode?: "value" | "percentage";
    additionalInsurance?: number; // Always a value (not percentage)
    completionPercentage?: number;
    productId?: string;
    productName?: string;
    notes?: string;
    procurementDate?: string;
    isDeleted?: boolean;
    deductions?: number;
    deserved?: number;
    net?: number;
    achievementPercentage?: number;
    transportationExpenses?: number;
    gratuities?: number;
}