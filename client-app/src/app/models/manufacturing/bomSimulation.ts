export interface BOMSimulation {
    productLevel?: number;
    productId?: string;
    productName?: string;
    quantity?: number;
    qoh?: number;
    cost?: number | null;
    totalCost?: number | null;
}