export interface CertificateItem {
    workEffortId: string; // Maps to WorkEffort.WorkEffortId
    workEffortParentId: string; // Maps to WorkEffort.WorkEffortParentId, links to parent certificate
    description: string; // Maps to WorkEffort.Description
    quantity: number; // Maps to WorkEffort.Quantity
    unitPrice: number; // Maps to WorkEffort.Rate
    totalAmount: number; // Maps to WorkEffort.TotalAmount
    completionPercentage?: number; // Maps to WorkEffort.CompletionPercentage
    productId?: string; // Maps to WorkEffort.ProductId
    notes?: string; // Maps to WorkEffort.Notes
    isDeleted?: boolean; // UI flag for soft deletion, not in WorkEffort
}