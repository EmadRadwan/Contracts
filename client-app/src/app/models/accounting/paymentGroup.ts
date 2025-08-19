export interface PaymentGroup {
    paymentGroupId: string
    paymentGroupTypeId: string
    paymentGroupTypeDescription: string
    paymentGroupName: string
    canGenerateDepositSlip: boolean
    canPrintCheck: boolean
    canCancel: boolean
}