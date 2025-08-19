export interface ProductionRunRoutingTask {
    workEffortId?: string;
    workEffortParentId?: string;
    workEffortName?: string;
    description?: string;
    quantityToProduce?: number;
    currentStatusId: string;
    currentStatusDescription?: string;
    estimatedStartDate?: Date;
    estimatedCompletionDate?: Date;
    estimatedSetupMillis?: number;
    estimatedMilliSeconds?: number;
    fixedAssetId?: string;
    fixedAssetName?: string;
    sequenceNum?: number;
    canStartTask?: boolean;
    canCompleteTask?: boolean;
    canDeclareTask?: boolean;
    isFinalTask?: boolean;
    areComponentsIssued?: boolean;
    quantityProduced?: number;
    quantityRejected?: number;
    canDeclareAndProduce?: string;
    canProduce?: string;
    lastLotId?: string;
}
