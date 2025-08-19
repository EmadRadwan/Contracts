export interface MaterialCost {
    productId: string;
    productName?: string
    quantity: number;
    uomId: string;
    estimatedUnitCost: number;
    costComponentTypeId: string;
    costUomId: string;
    fromDate: string | Date;
    thruDate: string | Date;
}