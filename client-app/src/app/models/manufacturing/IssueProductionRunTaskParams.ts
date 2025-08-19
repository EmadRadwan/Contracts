export interface IssueProductionRunTaskParams {
    workEffortId?: string;
    reserveOrderEnumId?: string | null;
    failIfItemsAreNotAvailable?: string;
    failIfItemsAreNotOnHand?: boolean;
}
