export interface CostComponent {
    costComponentId: string;
    costComponentTypeId: string;
    costComponentTypeDescription: string;
    workEffortId?: string;
    workEffortName?: string;
    cost?: number;
    costUomId?: string;
    fromDate?: string;
    thruDate?: string;
    children?: CostComponent[];
    expanded?: boolean;
}