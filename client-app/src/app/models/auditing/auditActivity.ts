// One command execution: who asked for what, and what came back.
export interface AuditActivity {
    activityId: string;
    startedAt: Date | string;
    userName?: string;
    userId?: string;
    requestName?: string;
    requestPath?: string;
    httpMethod?: string;
    clientIpAddress?: string;
    isSuccess: boolean;
    errorMessage?: string;
    exceptionType?: string;
    durationMs?: number;
    // Ties this action to the rows it changed (EntityAuditLog.changedSessionInfo).
    correlationId?: string;
    requestJson?: string;
}
