export interface CostComponent {
    costComponentId?: string;
    costComponentTypeId?: string;
    productId?: string;
    productFeatureId?: string;
    partyId?: string;
    geoId?: string;
    workEffortId?: string;
    fixedAssetId?: string;
    costComponentCalcId?: string;
    fromDate?: Date;
    thruDate?: Date;
    cost?: number;
    costUomId?: string;
    expanded?: boolean
    children?: CostComponent[]
}
