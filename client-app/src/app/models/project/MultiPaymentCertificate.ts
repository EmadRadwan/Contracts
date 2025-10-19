export enum MultiPaymentCertificateStatus {
    CREATED = "CREATED",
    APPROVED = "APPROVED",
}

export interface MultiPaymentCertificate {
    workEffortId: string;
    code: string;
    date: string;
    chequeDate: string;
    description: string;
    chequeNumber: string;
    paymentMethodId: string;
    paymentMethod?: string; 
    totalAmount?: number; 
    currentStatusId?: MultiPaymentCertificateStatus;
}