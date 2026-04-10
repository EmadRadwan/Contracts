export interface MultiPaymentItem {
    workEffortId: string;
    workEffortIdParent?: string;
    glAccountId: string;
    glAccountName?: string;
    itemType: string;
    itemTypeDescription?: string;
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
    partyIdSupplierName?: string;
    partyIdContractor: string;
    partyIdContractorName?: string;
}