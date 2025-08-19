export interface CostComponentCalc {
    costComponentCalcId?: string;
    description?: string;
    costGlAccountTypeId?: string;
    productId: string
    workEffortId: string
    offsettingGlAccountTypeId?: string;
    fixedCost?: number;
    variableCost?: number;
    perMilliSecond?: number;
    currencyUomId?: string;
    costCustomMethodId?: string;
    costComponentTypeId?: string
    costComponentTypeDescription?: string
    fromDate: string;
    thruDate?: string
}
