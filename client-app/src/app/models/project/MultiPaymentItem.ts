export interface MultiPaymentItem {
    workEffortId: string;
    glAccountId: string;
    itemType: string;
    serviceId: string;
    serviceName: string;
    productId: string;
    productName: string;
    description: string;
    amount: number;
    discount: number;
    discountMode: "value" | "percentage";
    transportationExpenses: number;
    gratuities: number;
    total: number;
    partyIdSupplier: string;
    partyIdContractor: string;
}