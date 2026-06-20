export interface ProjectCommissionRate {
    projectCommissionRateId: string;
    projectId: string;
    projectName?: string;
    saleTypeId: string;
    salesRepPercent: number;
    managerPercent: number;
    externalCompanyPercent?: number | null;
    externalSalesRepPercent?: number | null;
    externalManagerPercent?: number | null;
    lastUpdatedStamp?: string;
    createdStamp?: string;
}
