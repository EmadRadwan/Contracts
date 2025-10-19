export interface MultiPaymentItem {
    itemId: string;
    workEffortId: string; // REFACTOR: Replaced certificateId with workEffortId to match the form's usage in initialValues and handleSubmit.
    projectId: string;
    projectName: string;
    subProjectId: string;
    subProjectName: string;
    itemType: string;
    productId: string;
    productName: string;
    description: string;
    amount: number;
    discount: number;
    discountMode: "value" | "percentage";
    transportationExpenses: number;
    gratuities: number;
    total: number;
    partyIdSupplier: string; // REFACTOR: Added partyIdSupplier to support the optional supplier field in the form.
    partyIdContractor: string; // REFACTOR: Added partyIdContractor to support the optional contractor field in the form.
    // REFACTOR: Removed uomId and uomName as they are not used in the form fields or logic, aligning the interface with actual form usage.
}