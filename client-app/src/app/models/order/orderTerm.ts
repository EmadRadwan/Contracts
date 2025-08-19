export interface OrderTerm {
    orderId: string
    termTypeId: string
    termTypeName: string
    orderItemSeqId?: string
    orderTermSeqId: string
    termValue?: number
    termDays?: number
    textValue?: string
    description?: string
    isNewTerm: string
}