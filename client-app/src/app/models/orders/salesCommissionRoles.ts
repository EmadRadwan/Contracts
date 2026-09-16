// Security roles for the Sales Commission feature. Frontend gating only for now —
// the API still relies on the global authenticated-user filter (backend enforcement
// is scheduled for the version-2 enhancements). Role names are seeded in
// Persistence/SeedContracts.cs; keep the two lists in sync.
export const SALES_COMMISSION_ROLES = {
    view: "ViewSalesCommission",
    create: "CreateSalesCommission",
    update: "UpdateSalesCommission",
    approve: "ApproveSalesCommission",
    reset: "ResetSalesCommission",
    delete: "DeleteSalesCommission",
} as const;

/** Any of these lets the user open the Sales Commissions list (read-only unless they also hold create/update). */
export const SALES_COMMISSION_ENTRY_ROLES: string[] = [
    SALES_COMMISSION_ROLES.view,
    SALES_COMMISSION_ROLES.create,
    SALES_COMMISSION_ROLES.update,
];

/** Any of these shows the Approve / Reset / Delete actions menu; each item is gated individually inside it. */
export const SALES_COMMISSION_ACTION_ROLES: string[] = [
    SALES_COMMISSION_ROLES.approve,
    SALES_COMMISSION_ROLES.reset,
    SALES_COMMISSION_ROLES.delete,
];
