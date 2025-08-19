export interface OrderPaymentPreference {
    orderId: string
    paymentMethodTypeId: string
    paymentMethodTypeDescription: string
    statusId: string
    statusDescription: string
    maxAmount: number
}