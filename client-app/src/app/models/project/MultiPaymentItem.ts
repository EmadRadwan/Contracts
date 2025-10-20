export interface MultiPaymentItem {
    itemId: string;
    workEffortId: string;
    projectId: string;
    projectName: string;
    subProjectId: string;
    subProjectName: string;
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