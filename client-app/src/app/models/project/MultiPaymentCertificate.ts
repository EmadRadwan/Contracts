import {MultiPaymentItem} from "./MultiPaymentItem";

export enum MultiPaymentCertificateStatus {
    CREATED = "CREATED",
    APPROVED = "APPROVED",
}

export interface MultiPaymentCertificate {
    workEffortId: string;
    date: string | Date;
    description: string;
    currentStatusId?: string;
    statusDescription?: string;
    statusDescriptionArabic?: string;
    glAccountId?: string;
    items: MultiPaymentItem[];
}

export interface FormInitialValues {
    workEffortId: string;
    date: Date;
    description: string;
    currentStatusId: string;
    statusDescription: string;
    statusDescriptionArabic: string;
    glAccountId: string;
}

export interface CertificateActionResult {
    success: boolean;
    certificate?: MultiPaymentCertificate;
}