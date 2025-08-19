export interface ActualProductCostComponentsTotalsDto {
    costComponents: CostComponent[];
    fohCostCount: number;
    directLaborCount: number;
    materialCostCount: number;
    total?: number;
}