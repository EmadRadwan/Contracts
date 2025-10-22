import {MultiPaymentItem} from "./MultiPaymentItem";

export enum MultiPaymentCertificateStatus {
    CREATED = "CREATED",
    APPROVED = "APPROVED",
}

export interface MultiPaymentCertificate {
    workEffortId: string;
    code: string;
    date: string | Date;
    description: string;
    paymentMethodId: string;
    chequeNumber?: string;
    chequeDate?: string | Date | null;
    currentStatusId?: string;
    statusDescription?: string;
    statusDescriptionArabic?: string;
    partyIdEmployee?: string;
    items: MultiPaymentItem[];
}