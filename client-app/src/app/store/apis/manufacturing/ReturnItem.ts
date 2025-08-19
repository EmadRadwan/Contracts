export interface ReturnItem {
    productId: string;
    workEffortId: string;
    fromDate: string | null;
    lotId: string;
    quantity: number;
}

export interface ReturnMaterialsRequest {
    productionRunId: string;
    items: ReturnItem[];
}

export interface ReturnMaterialsResponse {
    status: string;
}