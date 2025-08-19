export interface AgreementTerm {
    agreementTermId: string
    termTypeId?: string
    termTypeDescription?: string
    agreementId?: string
    agreementItemSeqId?: string
    invoiceItemTypeId?: string
    invoiceItemTypeDescription?: string
    fromDate?: Date | string | null
    thruDate?: Date | string | null
    termValue?: string
    termDays?: number | null
    textValue?: string
    minQuantity?: number | null
    maxQuantity?: number | null
    description?: string
}