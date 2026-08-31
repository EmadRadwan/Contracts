// One changed field. changedFieldName is "*CREATE*" or "*DELETE*" for inserts and deletes.
export interface EntityAuditLog {
    auditHistorySeqId: string;
    changedEntityName?: string;
    changedFieldName?: string;
    pkCombinedValueText?: string;
    oldValueText?: string;
    newValueText?: string;
    changedDate?: Date | string;
    changedByInfo?: string;
    changedSessionInfo?: string;
}
